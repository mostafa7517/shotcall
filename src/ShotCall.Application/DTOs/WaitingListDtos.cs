namespace ShotCall.Application.DTOs;

public class WaitingListEntryDto
{
    public Guid Id { get; set; }
    public Guid MatchId { get; set; }
    public Guid PhotographerId { get; set; }
    public string PhotographerName { get; set; } = string.Empty;
    public int Position { get; set; }
    public DateTime CreatedAt { get; set; }
}