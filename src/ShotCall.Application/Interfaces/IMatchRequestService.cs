using ShotCall.Application.DTOs;

namespace ShotCall.Application.Interfaces;

public interface IMatchRequestService
{
    Task<List<MatchRequestDto>> GetAllAsync(MatchRequestFilter filter);
    Task<MatchRequestDto> CreateAsync(CreateMatchRequestDto request, Guid photographerId);
    Task<MatchRequestDto?> WithdrawAsync(Guid requestId, Guid photographerId);
    Task<MatchRequestDto?> AcceptAsync(Guid requestId);
    Task<MatchRequestDto?> RejectAsync(Guid requestId);
}