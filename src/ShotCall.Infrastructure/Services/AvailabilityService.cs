using Microsoft.EntityFrameworkCore;
using ShotCall.Application.DTOs;
using ShotCall.Application.Interfaces;
using ShotCall.Domain.Entities;
using ShotCall.Infrastructure.Persistence;

namespace ShotCall.Infrastructure.Services;

public class AvailabilityService : IAvailabilityService
{
    private readonly AppDbContext _db;

    public AvailabilityService(AppDbContext db) => _db = db;

    public async Task<List<AvailabilityDto>> GetForPhotographerAsync(Guid photographerId)
    {
        var items = await _db.Availabilities
            .Where(a => a.PhotographerId == photographerId)
            .OrderBy(a => a.Date).ThenBy(a => a.FromTime)
            .ToListAsync();

        return items.Select(a => new AvailabilityDto
        {
            Id = a.Id,
            Date = a.Date,
            FromTime = a.FromTime,
            ToTime = a.ToTime
        }).ToList();
    }

    public async Task<AvailabilityDto> CreateAsync(Guid photographerId, CreateAvailabilityDto request)
    {
        var availability = new Availability
        {
            Id = Guid.NewGuid(),
            PhotographerId = photographerId,
            Date = request.Date,
            FromTime = request.FromTime,
            ToTime = request.ToTime
        };

        _db.Availabilities.Add(availability);
        await _db.SaveChangesAsync();

        return new AvailabilityDto
        {
            Id = availability.Id,
            Date = availability.Date,
            FromTime = availability.FromTime,
            ToTime = availability.ToTime
        };
    }

    public async Task<bool> DeleteAsync(Guid id, Guid photographerId)
    {
        var availability = await _db.Availabilities
            .FirstOrDefaultAsync(a => a.Id == id && a.PhotographerId == photographerId);
        if (availability is null) return false;

        _db.Availabilities.Remove(availability);
        await _db.SaveChangesAsync();
        return true;
    }
}