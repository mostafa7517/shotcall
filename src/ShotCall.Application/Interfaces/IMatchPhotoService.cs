using ShotCall.Application.DTOs;

namespace ShotCall.Application.Interfaces;

public interface IMatchPhotoService
{
    Task<List<MatchPhotoDto>> GetByMatchAsync(Guid matchId);
    Task<MatchPhotoDto> CreateAsync(Guid matchId, Guid photographerId, string fileUrl);
}