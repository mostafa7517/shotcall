namespace ShotCall.Application.DTOs;

public class AccountDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Bio { get; set; }
    public string Role { get; set; } = string.Empty;
    public string AccountStatus { get; set; } = string.Empty;
    public int ReliabilityScore { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AccountFilter
{
    public string? Status { get; set; } // "PendingApproval" / "Active" / "Rejected" / "Disabled"
}