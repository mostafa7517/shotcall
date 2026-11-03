using Microsoft.EntityFrameworkCore;
using ShotCall.Application.DTOs;
using ShotCall.Application.Interfaces;
using ShotCall.Domain.Entities;
using ShotCall.Infrastructure.Persistence;

namespace ShotCall.Infrastructure.Services;

public class MatchPhotoService : IMatchPhotoService
{
    private readonly AppDbContext _db;

    public MatchPhotoService(AppDbContext db) => _db = db;

    public async Task<List<MatchPhotoDto>> GetByMatchAsync(Guid matchId)
    {
        var photos = await _db.MatchPhotos
            .Include(p => p.Photographer)
            .Where(p => p.MatchId == matchId)
            .OrderByDescending(p => p.UploadedAt)
            .ToListAsync();

        return photos.Select(ToDto).ToList();
    }

    public async Task<MatchPhotoDto> CreateAsync(Guid matchId, Guid photographerId, string fileUrl)
    {
        var matchExists = await _db.Matches.AnyAsync(m => m.Id == matchId);
        if (!matchExists) throw new InvalidOperationException("Match not found.");

        var photo = new MatchPhoto
        {
            Id = Guid.NewGuid(),
            MatchId = matchId,
            PhotographerId = photographerId,
            FileUrl = fileUrl,
            UploadedAt = DateTime.UtcNow
        };

        _db.MatchPhotos.Add(photo);
        await _db.SaveChangesAsync();
        await _db.Entry(photo).Reference(p => p.Photographer).LoadAsync();

        return ToDto(photo);
    }

    private static MatchPhotoDto ToDto(MatchPhoto p) => new()
    {
        Id = p.Id,
        MatchId = p.MatchId,
        PhotographerId = p.PhotographerId,
        PhotographerName = p.Photographer.FullName,
        FileUrl = p.FileUrl,
        UploadedAt = p.UploadedAt
    };
}