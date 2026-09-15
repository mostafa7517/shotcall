using Microsoft.EntityFrameworkCore;
using ShotCall.Application.DTOs;
using ShotCall.Application.Interfaces;
using ShotCall.Domain.Entities;
using ShotCall.Infrastructure.Persistence;

namespace ShotCall.Infrastructure.Services;

public class MatchCommentService : IMatchCommentService
{
    private readonly AppDbContext _db;

    public MatchCommentService(AppDbContext db) => _db = db;

    public async Task<List<MatchCommentDto>> GetByMatchAsync(Guid matchId)
    {
        var comments = await _db.MatchComments
            .Include(c => c.Author)
            .Where(c => c.MatchId == matchId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        return comments.Select(ToDto).ToList();
    }

    public async Task<MatchCommentDto> CreateAsync(Guid matchId, Guid authorId, string message)
    {
        var matchExists = await _db.Matches.AnyAsync(m => m.Id == matchId);
        if (!matchExists) throw new InvalidOperationException("Match not found.");

        var comment = new MatchComment
        {
            Id = Guid.NewGuid(),
            MatchId = matchId,
            AuthorId = authorId,
            Message = message,
            CreatedAt = DateTime.UtcNow
        };

        _db.MatchComments.Add(comment);
        await _db.SaveChangesAsync();
        await _db.Entry(comment).Reference(c => c.Author).LoadAsync();

        return ToDto(comment);
    }

    private static MatchCommentDto ToDto(MatchComment c) => new()
    {
        Id = c.Id,
        MatchId = c.MatchId,
        AuthorId = c.AuthorId,
        AuthorName = c.Author.FullName,
        Message = c.Message,
        CreatedAt = c.CreatedAt
    };
}