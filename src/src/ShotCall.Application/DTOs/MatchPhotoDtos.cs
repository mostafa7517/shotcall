namespace ShotCall.Application.DTOs;

public class MatchPhotoDto
{
    public Guid Id { get; set; }
    public Guid MatchId { get; set; }
    public Guid PhotographerId { get; set; }
    public string PhotographerName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}

public class CreateMatchPhotoDto
{
    public string FileUrl { get; set; } = string.Empty;
}