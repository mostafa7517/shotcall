namespace ShotCall.Application.DTOs;

public class CancellationRequestDto
{
    public Guid Id { get; set; }
    public Guid MatchRequestId { get; set; }
    public string MatchTitle { get; set; } = string.Empty;
    public string PhotographerName { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public bool IsLateCancellation { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
}

public class CreateCancellationRequestDto
{
    public Guid MatchRequestId { get; set; }
    public string? Reason { get; set; }
}