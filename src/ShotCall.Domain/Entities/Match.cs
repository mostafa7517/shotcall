using ShotCall.Domain.Enums;

namespace ShotCall.Domain.Entities;

public class Match
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? TeamA { get; set; }
    public string? TeamB { get; set; }
    public string Category { get; set; } = string.Empty; // e.g. Football, Padel, Tennis, Press Conference

    // An event (no teams) instead of a match between two sides
    public bool IsEvent { get; set; }

    // The MC / host of the event (only used when IsEvent is true)
    public string? HostName { get; set; }

    public DateTime MatchDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? Description { get; set; }

    public DateTime? RequestDeadline { get; set; }

    public MatchStatus Status { get; set; } = MatchStatus.Open;

    public Guid CreatedByAdminId { get; set; }
    public ApplicationUser CreatedByAdmin { get; set; } = null!;

    // Navigation
    public ICollection<MatchSlot> Slots { get; set; } = new List<MatchSlot>();
    public ICollection<MatchRequest> Requests { get; set; } = new List<MatchRequest>();
    public ICollection<WaitingListEntry> WaitingList { get; set; } = new List<WaitingListEntry>();
    public ICollection<MatchComment> Comments { get; set; } = new List<MatchComment>();
    public ICollection<MatchPhoto> Photos { get; set; } = new List<MatchPhoto>();

    // People who confirmed
    public int AcceptedCountFor(StaffType type) =>
        Requests.Count(r => r.StaffType == type && r.Status == RequestStatus.Accepted);

    // People an admin invited who haven't answered yet
    public int InvitedCountFor(StaffType type) =>
        Requests.Count(r => r.StaffType == type && r.Status == RequestStatus.Invited);

    // Places that are no longer free: confirmed + invited (an invitation holds the place)
    public int ReservedCountFor(StaffType type) =>
        Requests.Count(r => r.StaffType == type &&
                            (r.Status == RequestStatus.Accepted || r.Status == RequestStatus.Invited));

    // True when every role slot has all its places taken (confirmed or held by an invitation)
    public bool IsFull() =>
        Slots.Count > 0 && Slots.All(s => ReservedCountFor(s.StaffType) >= s.Needed);
}
