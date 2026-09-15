namespace ShotCall.Application.DTOs;

public class MatchDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? TeamA { get; set; }
    public string? TeamB { get; set; }
    public string Category { get; set; } = string.Empty;
    public DateTime MatchDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int PhotographersNeeded { get; set; }
    public int AcceptedCount { get; set; }
    public DateTime? RequestDeadline { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class CreateMatchRequest
{
    public string Title { get; set; } = string.Empty;
    public string? TeamA { get; set; }
    public string? TeamB { get; set; }
    public string Category { get; set; } = string.Empty;
    public DateTime MatchDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int PhotographersNeeded { get; set; } = 1;
    public DateTime? RequestDeadline { get; set; }
}

public class UpdateMatchRequest
{
    public string Title { get; set; } = string.Empty;
    public string? TeamA { get; set; }
    public string? TeamB { get; set; }
    public string Category { get; set; } = string.Empty;
    public DateTime MatchDate { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int PhotographersNeeded { get; set; }
    public DateTime? RequestDeadline { get; set; }
}

public class MatchFilter
{
    public string? Category { get; set; }
    public string? Status { get; set; } // "Open" / "Closed"
}