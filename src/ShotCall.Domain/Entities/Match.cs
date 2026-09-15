using ShotCall.Domain.Enums;

namespace ShotCall.Domain.Entities;

public class Match
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? TeamA { get; set; }
    public string? TeamB { get; set; }
    public string Category { get; set; } = string.Empty; // e.g. Football, Padel

    public DateTime MatchDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? Description { get; set; }

    public int PhotographersNeeded { get; set; } = 1;
    public DateTime? RequestDeadline { get; set; }

    public MatchStatus Status { get; set; } = MatchStatus.Open;

    public Guid CreatedByAdminId { get; set; }
    public ApplicationUser CreatedByAdmin { get; set; } = null!;

    // Navigation
    public ICollection<MatchRequest> Requests { get; set; } = new List<MatchRequest>();
    public ICollection<WaitingListEntry> WaitingList { get; set; } = new List<WaitingListEntry>();
    public ICollection<MatchComment> Comments { get; set; } = new List<MatchComment>();
    public ICollection<MatchPhoto> Photos { get; set; } = new List<MatchPhoto>();

    public int AcceptedCount => Requests.Count(r => r.Status == RequestStatus.Accepted);
}