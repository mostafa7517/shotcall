using Microsoft.EntityFrameworkCore;
using ShotCall.Application.DTOs;
using ShotCall.Application.Interfaces;
using ShotCall.Domain.Entities;
using ShotCall.Infrastructure.Persistence;

namespace ShotCall.Infrastructure.Services;

public class WaitingListService : IWaitingListService
{
    private readonly AppDbContext _db;

    public WaitingListService(AppDbContext db) => _db = db;

    public async Task<List<WaitingListEntryDto>> GetForMatchAsync(Guid matchId)
    {
        var items = await _db.WaitingListEntries
            .Include(w => w.Photographer)
            .Where(w => w.MatchId == matchId)
            .OrderBy(w => w.Position)
            .ToListAsync();

        return items.Select(ToDto).ToList();
    }

    public async Task<WaitingListEntryDto> JoinAsync(Guid matchId, Guid photographerId)
    {
        var matchExists = await _db.Matches.AnyAsync(m => m.Id == matchId);
        if (!matchExists) throw new InvalidOperationException("Match not found.");

        var alreadyWaiting = await _db.WaitingListEntries
            .AnyAsync(w => w.MatchId == matchId && w.PhotographerId == photographerId);
        if (alreadyWaiting)
            throw new InvalidOperationException("You are already on the waiting list for this match.");

        var nextPosition = await _db.WaitingListEntries
            .Where(w => w.MatchId == matchId)
            .CountAsync() + 1;

        var entry = new WaitingListEntry
        {
            Id = Guid.NewGuid(),
            MatchId = matchId,
            PhotographerId = photographerId,
            Position = nextPosition,
            CreatedAt = DateTime.UtcNow
        };

        _db.WaitingListEntries.Add(entry);
        await _db.SaveChangesAsync();
        await _db.Entry(entry).Reference(w => w.Photographer).LoadAsync();

        return ToDto(entry);
    }

    private static WaitingListEntryDto ToDto(WaitingListEntry w) => new()
    {
        Id = w.Id,
        MatchId = w.MatchId,
        PhotographerId = w.PhotographerId,
        PhotographerName = w.Photographer.FullName,
        Position = w.Position,
        CreatedAt = w.CreatedAt
    };
}