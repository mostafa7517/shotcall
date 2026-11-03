namespace ShotCall.Application.DTOs;

public class ViolationDto
{
    public Guid Id { get; set; }
    public Guid PhotographerId { get; set; }
    public string PhotographerName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateViolationDto
{
    public Guid PhotographerId { get; set; }
    public Guid? MatchId { get; set; }
    public string Type { get; set; } = "Other"; // NoShow / LateCancellation / Other
    public string? Notes { get; set; }
}