namespace ShotCall.Application.DTOs;

// One person the admin could appoint for a match, with what is needed to decide
public class AssignCandidateDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public string StaffType { get; set; } = string.Empty;
    public int ReliabilityScore { get; set; }

    // "Pending" / "Accepted" / "Invited" when the person is already on this match, otherwise null
    public string? RequestStatus { get; set; }

    // Titles of other matches this person is already booked for around the same time
    public List<string> Conflicts { get; set; } = new();
}

public class InviteStaffRequest
{
    public List<Guid> UserIds { get; set; } = new();
}

public class DeclineInvitationRequest
{
    public string? Reason { get; set; }
}
