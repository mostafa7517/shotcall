namespace ShotCall.Application.DTOs;

public class MatchSlotDto
{
    public string StaffType { get; set; } = string.Empty;
    public int Needed { get; set; }

    // People who confirmed
    public int Accepted { get; set; }

    // People invited by an admin who haven't answered yet (they hold a place)
    public int Invited { get; set; }
}

public class MatchSlotInput
{
    public string StaffType { get; set; } = string.Empty;
    public int Needed { get; set; }
}

public class MatchDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? TeamA { get; set; }
    public string? TeamB { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsEvent { get; set; }
    public string? HostName { get; set; }
    public DateTime MatchDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<MatchSlotDto> Slots { get; set; } = new();
    public int NeededTotal { get; set; }
    public int AcceptedTotal { get; set; }
    public int InvitedTotal { get; set; }
    public DateTime? RequestDeadline { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class CreateMatchRequest
{
    public string Title { get; set; } = string.Empty;
    public string? TeamA { get; set; }
    public string? TeamB { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsEvent { get; set; }
    public string? HostName { get; set; }
    public DateTime MatchDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<MatchSlotInput> Slots { get; set; } = new();
    public DateTime? RequestDeadline { get; set; }
}

public class UpdateMatchRequest
{
    public string Title { get; set; } = string.Empty;
    public string? TeamA { get; set; }
    public string? TeamB { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsEvent { get; set; }
    public string? HostName { get; set; }
    public DateTime MatchDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<MatchSlotInput> Slots { get; set; } = new();
    public DateTime? RequestDeadline { get; set; }
}

public class MatchFilter
{
    public string? Category { get; set; }
    public string? Status { get; set; }    // "Open" / "Closed"
    public string? StaffType { get; set; } // only matches that need this role
}
