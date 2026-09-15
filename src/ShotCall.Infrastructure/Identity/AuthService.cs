using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ShotCall.Application.DTOs;
using ShotCall.Application.Interfaces;
using ShotCall.Domain.Entities;
using ShotCall.Domain.Enums;
using ShotCall.Infrastructure.Persistence;

namespace ShotCall.Infrastructure.Identity;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly IJwtService _jwtService;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext db, IJwtService jwtService, IConfiguration config)
    {
        _db = db;
        _jwtService = jwtService;
        _config = config;
    }

    public async Task<AuthResponseDto> GoogleLoginAsync(string googleIdToken)
    {
        var settings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = new[] { _config["Google:ClientId"]! }
        };

        GoogleJsonWebSignature.Payload payload;
        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(googleIdToken, settings);
        }
        catch (InvalidJwtException)
        {
            throw new InvalidOperationException("Invalid Google token.");
        }

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == payload.Email);

        if (user is null)
        {
            var adminEmails = _config.GetSection("AdminEmails").Get<string[]>() ?? Array.Empty<string>();
            var isAdmin = adminEmails.Contains(payload.Email, StringComparer.OrdinalIgnoreCase);

            user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                FullName = payload.Name ?? payload.Email,
                Email = payload.Email,
                ProfilePictureUrl = payload.Picture,
                Role = isAdmin ? UserRole.Admin : UserRole.Photographer,
                AccountStatus = isAdmin ? AccountStatus.Active : AccountStatus.PendingApproval,
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // TODO: notify all admins about the new pending account once notification service is wired in here
        }

        if (user.AccountStatus is AccountStatus.Rejected or AccountStatus.Disabled)
            throw new InvalidOperationException("Your account is not active. Contact the admin.");

        return await IssueTokensAsync(user);
    }

    public async Task<AuthResponseDto?> RefreshAsync(string refreshToken)
    {
        var stored = await _db.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == refreshToken);

        if (stored is null || stored.IsRevoked || stored.ExpiresAt < DateTime.UtcNow)
            return null;

        // Rotate: revoke the old one, issue a new pair
        stored.IsRevoked = true;
        await _db.SaveChangesAsync();

        return await IssueTokensAsync(stored.User);
    }

    private async Task<AuthResponseDto> IssueTokensAsync(ApplicationUser user)
    {
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();
        var refreshDays = int.Parse(_config["Jwt:RefreshTokenDays"] ?? "14");

        _db.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshDays),
            IsRevoked = false
        });
        await _db.SaveChangesAsync();

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            AccountStatus = user.AccountStatus.ToString()
        };
    }
}