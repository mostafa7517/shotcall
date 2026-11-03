using Microsoft.EntityFrameworkCore;
using ShotCall.Application.DTOs;
using ShotCall.Application.Interfaces;
using ShotCall.Domain.Entities;
using ShotCall.Domain.Enums;
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
            .OrderBy(w => w.StaffType).ThenBy(w => w.Position)
            .ToListAsync();

        return items.Select(ToDto).ToList();
    }

    public async Task<WaitingListEntryDto> JoinAsync(Guid matchId, Guid photographerId)
    {
        var user = await _db.Users.FindAsync(photographerId)
            ?? throw new InvalidOperationException("Account not found.");

        if (user.StaffType is null)
            throw new InvalidOperationException("Choose your role in your profile first.");

        var type = user.StaffType.Value;

        var match = await _db.Matches
            .Include(m => m.Slots)
            .Include(m => m.Requests)
            .FirstOrDefaultAsync(m => m.Id == matchId)
            ?? throw new InvalidOperationException("Match not found.");

        var slot = match.Slots.FirstOrDefault(s => s.StaffType == type);
        if (slot is null)
            throw new InvalidOperationException($"This match isn't asking for your role ({type}).");

        if (match.AcceptedCountFor(type) < slot.Needed)
            throw new InvalidOperationException("There are still open spots for your role. Request the match instead.");

        var hasRequest = match.Requests.Any(r =>
            r.PhotographerId == photographerId &&
            r.Status is RequestStatus.Pending or RequestStatus.Accepted);
        if (hasRequest)
            throw new InvalidOperationException("You already have a request for this match.");

        var alreadyWaiting = await _db.WaitingListEntries
            .AnyAsync(w => w.MatchId == matchId && w.PhotographerId == photographerId);
        if (alreadyWaiting)
            throw new InvalidOperationException("You are already on the waiting list for this match.");

        var lastPosition = await _db.WaitingListEntries
            .Where(w => w.MatchId == matchId && w.StaffType == type)
            .MaxAsync(w => (int?)w.Position) ?? 0;

        var entry = new WaitingListEntry
        {
            Id = Guid.NewGuid(),
            MatchId = matchId,
            PhotographerId = photographerId,
            StaffType = type,
            Position = lastPosition + 1,
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
        StaffType = w.StaffType.ToString(),
        Position = w.Position,
        CreatedAt = w.CreatedAt
    };
}