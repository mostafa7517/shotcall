namespace ShotCall.Domain.Entities;

public class GmailAuthorization
{
    public Guid Id { get; set; }

    public Guid AdminUserId { get; set; }
    public ApplicationUser AdminUser { get; set; } = null!;

    public string EncryptedRefreshToken { get; set; } = string.Empty;
    public string ConnectedEmail { get; set; } = string.Empty;

    public DateTime ConnectedAt { get; set; } = DateTime.UtcNow;
}