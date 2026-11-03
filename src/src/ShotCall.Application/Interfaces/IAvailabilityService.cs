using ShotCall.Application.DTOs;

namespace ShotCall.Application.Interfaces;

public interface IAvailabilityService
{
    Task<List<AvailabilityDto>> GetForPhotographerAsync(Guid photographerId);
    Task<AvailabilityDto> CreateAsync(Guid photographerId, CreateAvailabilityDto request);
    Task<bool> DeleteAsync(Guid id, Guid photographerId);
}