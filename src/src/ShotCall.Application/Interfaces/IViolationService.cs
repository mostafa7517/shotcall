using ShotCall.Application.DTOs;

namespace ShotCall.Application.Interfaces;

public interface IViolationService
{
    Task<List<ViolationDto>> GetForPhotographerAsync(Guid photographerId);
    Task<ViolationDto> CreateAsync(CreateViolationDto request);
}