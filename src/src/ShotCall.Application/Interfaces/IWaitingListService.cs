using ShotCall.Application.DTOs;

namespace ShotCall.Application.Interfaces;

public interface IWaitingListService
{
    Task<List<WaitingListEntryDto>> GetForMatchAsync(Guid matchId);
    Task<WaitingListEntryDto> JoinAsync(Guid matchId, Guid photographerId);
}