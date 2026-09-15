using Microsoft.EntityFrameworkCore;
using ShotCall.Application.DTOs;
using ShotCall.Application.Interfaces;
using ShotCall.Domain.Entities;
using ShotCall.Domain.Enums;
using ShotCall.Infrastructure.Persistence;

namespace ShotCall.Infrastructure.Services;

public class ViolationService : IViolationService
{
    private readonly AppDbContext _db;

    public ViolationService(AppDbContext db) => _db = db;

    public async Task<List<ViolationDto>> GetForPhotographerAsync(Guid photographerId)
    {
        var items = await _db.Violations
            .Include(v => v.Photographer)
            .Where(v => v.PhotographerId == photographerId)
            .OrderByDescending(v => v.CreatedAt)
            .ToListAsync();

        return items.Select(ToDto).ToList();
    }

    public async Task<ViolationDto> CreateAsync(CreateViolationDto request)
    {
        var photographer = await _db.Users.FindAsync(request.PhotographerId)
            ?? throw new InvalidOperationException("Photographer not found.");

        if (!Enum.TryParse<ViolationType>(request.Type, true, out var type))
            type = ViolationType.Other;

        var violation = new Violation
        {
            Id = Guid.NewGuid(),
            PhotographerId = request.PhotographerId,
            MatchId = request.MatchId,
            Type = type,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _db.Violations.Add(violation);

        // Simple reliability penalty - adjust the weighting later if needed
        photographer.ReliabilityScore = Math.Max(0, photographer.ReliabilityScore - 10);

        await _db.SaveChangesAsync();

        return ToDto(violation, photographer);
    }

    private static ViolationDto ToDto(Violation v) => ToDto(v, v.Photographer);

    private static ViolationDto ToDto(Violation v, ApplicationUser photographer) => new()
    {
        Id = v.Id,
        PhotographerId = v.PhotographerId,
        PhotographerName = photographer.FullName,
        Type = v.Type.ToString(),
        Notes = v.Notes,
        CreatedAt = v.CreatedAt
    };
}