using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ShotCall.Application.Interfaces;
using ShotCall.Domain.Entities;
using ShotCall.Infrastructure.Persistence;

namespace ShotCall.Infrastructure.Identity;

public class GmailAuthService : IGmailAuthService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly IDataProtector _protector;
    private readonly HttpClient _httpClient;

    public GmailAuthService(AppDbContext db, IConfiguration config, IDataProtectionProvider dataProtectionProvider, IHttpClientFactory httpClientFactory)
    {
        _db = db;
        _config = config;
        _protector = dataProtectionProvider.CreateProtector("GmailRefreshToken");
        _httpClient = httpClientFactory.CreateClient();
    }

    public string GetAuthorizationUrl()
    {
        var clientId = _config["Google:ClientId"];
        var redirectUri = _config["Google:RedirectUri"];
        const string scope = "https://www.googleapis.com/auth/gmail.send";

        return "https://accounts.google.com/o/oauth2/v2/auth" +
               $"?client_id={Uri.EscapeDataString(clientId!)}" +
               $"&redirect_uri={Uri.EscapeDataString(redirectUri!)}" +
               $"&response_type=code" +
               $"&scope={Uri.EscapeDataString(scope)}" +
               "&access_type=offline" +
               "&prompt=consent";
    }

    public async Task<string> HandleCallbackAsync(string code, Guid adminUserId)
    {
        var clientId = _config["Google:ClientId"];
        var clientSecret = _config["Google:ClientSecret"];
        var redirectUri = _config["Google:RedirectUri"];

        var tokenResponse = await _httpClient.PostAsync("https://oauth2.googleapis.com/token",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["code"] = code,
                ["client_id"] = clientId!,
                ["client_secret"] = clientSecret!,
                ["redirect_uri"] = redirectUri!,
                ["grant_type"] = "authorization_code"
            }));

        var json = await tokenResponse.Content.ReadAsStringAsync();
        if (!tokenResponse.IsSuccessStatusCode)
            throw new InvalidOperationException($"Google token exchange failed: {json}");

        using var doc = System.Text.Json.JsonDocument.Parse(json);
        var refreshToken = doc.RootElement.GetProperty("refresh_token").GetString()
            ?? throw new InvalidOperationException("Google did not return a refresh token. Try disconnecting the app from your Google account and reconnecting.");

        var encrypted = _protector.Protect(refreshToken);

        var existing = await _db.GmailAuthorizations.FirstOrDefaultAsync(g => g.AdminUserId == adminUserId);
        if (existing is not null)
        {
            existing.EncryptedRefreshToken = encrypted;
            existing.ConnectedAt = DateTime.UtcNow;
        }
        else
        {
            _db.GmailAuthorizations.Add(new GmailAuthorization
            {
                Id = Guid.NewGuid(),
                AdminUserId = adminUserId,
                EncryptedRefreshToken = encrypted,
                ConnectedEmail = "", // filled in by GmailEmailService on first use if needed
                ConnectedAt = DateTime.UtcNow
            });
        }

        await _db.SaveChangesAsync();
        return "Gmail connected successfully.";
    }
}