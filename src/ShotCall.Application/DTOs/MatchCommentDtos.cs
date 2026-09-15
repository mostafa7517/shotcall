namespace ShotCall.Application.DTOs;

public class MatchCommentDto
{
    public Guid Id { get; set; }
    public Guid MatchId { get; set; }
    public Guid AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateMatchCommentDto
{
    public string Message { get; set; } = string.Empty;
}