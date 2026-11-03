using ShotCall.Domain.Enums;

namespace ShotCall.Domain.Entities;

public class Violation
{
    public Guid Id { get; set; }

    public Guid PhotographerId { get; set; }
    public ApplicationUser Photographer { get; set; } = null!;

    public Guid? MatchId { get; set; }
    public Match? Match { get; set; }

    public ViolationType Type { get; set; }
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}