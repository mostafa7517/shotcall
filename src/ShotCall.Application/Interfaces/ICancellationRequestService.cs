using ShotCall.Application.DTOs;

namespace ShotCall.Application.Interfaces;

public interface ICancellationRequestService
{
    Task<List<CancellationRequestDto>> GetPendingAsync();
    Task<CancellationRequestDto> CreateAsync(CreateCancellationRequestDto request, Guid photographerId);
    Task<CancellationRequestDto?> ApproveAsync(Guid id);
    Task<CancellationRequestDto?> RejectAsync(Guid id);
}