namespace ShotCall.Domain.Entities;

public class WaitingListEntry
{
    public Guid Id { get; set; }

    public Guid MatchId { get; set; }
    public Match Match { get; set; } = null!;

    public Guid PhotographerId { get; set; }
    public ApplicationUser Photographer { get; set; } = null!;

    public int Position { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}