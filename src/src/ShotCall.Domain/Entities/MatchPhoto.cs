namespace ShotCall.Domain.Entities;

public class MatchPhoto
{
    public Guid Id { get; set; }

    public Guid MatchId { get; set; }
    public Match Match { get; set; } = null!;

    public Guid PhotographerId { get; set; }
    public ApplicationUser Photographer { get; set; } = null!;

    public string FileUrl { get; set; } = string.Empty;

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}