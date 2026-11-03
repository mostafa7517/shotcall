using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ShotCall.Application.Interfaces;
using ShotCall.Infrastructure.Persistence;
using System.Text;

namespace ShotCall.Infrastructure.Services;

public class GmailEmailService : IEmailService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly IDataProtector _protector;

    public GmailEmailService(AppDbContext db, IConfiguration config, IDataProtectionProvider dataProtectionProvider)
    {
        _db = db;
        _config = config;
        _protector = dataProtectionProvider.CreateProtector("GmailRefreshToken");
    }

    public bool IsConnected => _db.GmailAuthorizations.Any();

    public async Task SendAsync(string toEmail, string subject, string bodyHtml)
    {
        var authorization = await _db.GmailAuthorizations.FirstOrDefaultAsync();
        if (authorization is null)
        {
            // Not connected yet - silently skip. In-app notifications still work.
            return;
        }

        var refreshToken = _protector.Unprotect(authorization.EncryptedRefreshToken);

        var tokenResponse = new Google.Apis.Auth.OAuth2.Responses.TokenResponse
        {
            RefreshToken = refreshToken
        };

        var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
        {
            ClientSecrets = new ClientSecrets
            {
                ClientId = _config["Google:ClientId"],
                ClientSecret = _config["Google:ClientSecret"]
            },
            Scopes = new[] { GmailService.Scope.GmailSend }
        });

        var credential = new UserCredential(flow, "admin", tokenResponse);
        await credential.RefreshTokenAsync(CancellationToken.None);

        var service = new GmailService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "ShotCall"
        });

        var rawMessage = BuildRawMessage(toEmail, subject, bodyHtml);

        var message = new Message { Raw = rawMessage };
        await service.Users.Messages.Send(message, "me").ExecuteAsync();
    }

    private static string BuildRawMessage(string toEmail, string subject, string bodyHtml)
    {
        var mime =
            $"To: {toEmail}\r\n" +
            $"Subject: {subject}\r\n" +
            "Content-Type: text/html; charset=utf-8\r\n\r\n" +
            bodyHtml;

        var bytes = Encoding.UTF8.GetBytes(mime);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .Replace("=", "");
    }
}