using ShotCall.Domain.Enums;

namespace ShotCall.Domain.Entities;

public class MatchRequest
{
    public Guid Id { get; set; }

    public Guid MatchId { get; set; }
    public Match Match { get; set; } = null!;

    public Guid PhotographerId { get; set; }
    public ApplicationUser Photographer { get; set; } = null!;

    public RequestStatus Status { get; set; } = RequestStatus.Pending;

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RespondedAt { get; set; }

    // Navigation
    public CancellationRequest? CancellationRequest { get; set; }
}