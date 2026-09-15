namespace ShotCall.Application.DTOs;

public class MatchRequestDto
{
    public Guid Id { get; set; }
    public Guid MatchId { get; set; }
    public string MatchTitle { get; set; } = string.Empty;
    public Guid PhotographerId { get; set; }
    public string PhotographerName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
}

public class CreateMatchRequestDto
{
    public Guid MatchId { get; set; }
}

public class MatchRequestFilter
{
    public string? Status { get; set; }
    public Guid? MatchId { get; set; }
    public Guid? PhotographerId { get; set; }
}