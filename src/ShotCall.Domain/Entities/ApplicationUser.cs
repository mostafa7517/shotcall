using ShotCall.Domain.Enums;

namespace ShotCall.Domain.Entities;

public class ApplicationUser
{
    public Guid Id { get; set; }

    // From Google on first login
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }

    // Filled in later by the user, all optional
    public string? PhoneNumber { get; set; }
    public string? Bio { get; set; }
    public string? PortfolioLinksJson { get; set; } // stored as JSON array of strings

    public UserRole Role { get; set; }
    public AccountStatus AccountStatus { get; set; } = AccountStatus.PendingApproval;

    public int ReliabilityScore { get; set; } = 100;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<Availability> Availabilities { get; set; } = new List<Availability>();
    public ICollection<MatchRequest> MatchRequests { get; set; } = new List<MatchRequest>();
    public ICollection<Violation> Violations { get; set; } = new List<Violation>();
}