namespace ShotCall.Application.Interfaces;

public interface IGmailAuthService
{
    string GetAuthorizationUrl();
    Task<string> HandleCallbackAsync(string code, Guid adminUserId);
}