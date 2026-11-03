using ShotCall.Application.DTOs;

namespace ShotCall.Application.Interfaces;

public interface IMatchService
{
    Task<List<MatchDto>> GetAllAsync(MatchFilter filter);
    Task<MatchDto?> GetByIdAsync(Guid id);
    Task<MatchDto> CreateAsync(CreateMatchRequest request, Guid adminId);
    Task<MatchDto?> UpdateAsync(Guid id, UpdateMatchRequest request);
    Task<bool> DeleteAsync(Guid id);
}