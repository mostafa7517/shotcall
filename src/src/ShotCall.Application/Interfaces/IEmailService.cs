namespace ShotCall.Application.Interfaces;

public interface IEmailService
{
    Task SendAsync(string toEmail, string subject, string bodyHtml);
    bool IsConnected { get; }
}