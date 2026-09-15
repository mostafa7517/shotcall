using ShotCall.Domain.Enums;

namespace ShotCall.Domain.Entities;

public class CancellationRequest
{
    public Guid Id { get; set; }

    public Guid MatchRequestId { get; set; }
    public MatchRequest MatchRequest { get; set; } = null!;

    public string? Reason { get; set; }
    public CancellationStatus Status { get; set; } = CancellationStatus.Pending;

    // True if submitted within the configurable threshold before MatchDate (e.g. < 48h)
    public bool IsLateCancellation { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RespondedAt { get; set; }
}