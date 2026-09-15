using ShotCall.Application.DTOs;

namespace ShotCall.Application.Interfaces;

public interface IMatchCommentService
{
    Task<List<MatchCommentDto>> GetByMatchAsync(Guid matchId);
    Task<MatchCommentDto> CreateAsync(Guid matchId, Guid authorId, string message);
}