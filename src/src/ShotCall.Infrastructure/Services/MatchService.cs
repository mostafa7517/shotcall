using Microsoft.EntityFrameworkCore;
using ShotCall.Application.DTOs;
using ShotCall.Application.Interfaces;
using ShotCall.Domain.Entities;
using ShotCall.Domain.Enums;
using ShotCall.Infrastructure.Persistence;

namespace ShotCall.Infrastructure.Services;

public class MatchService : IMatchService
{
    private readonly AppDbContext _db;
    private readonly INotificationService _notificationService;

    public MatchService(AppDbContext db, INotificationService notificationService)
    {
        _db = db;
        _notificationService = notificationService;
    }

    public async Task<List<MatchDto>> GetAllAsync(MatchFilter filter)
    {
        var query = _db.Matches
            .Include(m => m.Slots)
            .Include(m => m.Requests)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Category))
            query = query.Where(m => m.Category == filter.Category);

        if (!string.IsNullOrWhiteSpace(filter.Status) &&
            Enum.TryParse<MatchStatus>(filter.Status, true, out var status))
            query = query.Where(m => m.Status == status);

        if (!string.IsNullOrWhiteSpace(filter.StaffType) &&
            Enum.TryParse<StaffType>(filter.StaffType, true, out var staffType))
            query = query.Where(m => m.Slots.Any(s => s.StaffType == staffType && s.Needed > 0));

        var matches = await query.OrderBy(m => m.MatchDate).ToListAsync();
        return matches.Select(ToDto).ToList();
    }

    public async Task<MatchDto?> GetByIdAsync(Guid id)
    {
        var match = await _db.Matches
            .Include(m => m.Slots)
            .Include(m => m.Requests)
            .FirstOrDefaultAsync(m => m.Id == id);

        return match is null ? null : ToDto(match);
    }

    public async Task<MatchDto> CreateAsync(CreateMatchRequest request, Guid adminId)
    {
        var slots = ParseSlots(request.Slots);

        var match = new Match
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            TeamA = request.TeamA,
            TeamB = request.TeamB,
            Category = request.Category,
            MatchDate = DateTime.SpecifyKind(request.MatchDate, DateTimeKind.Utc),
            Location = request.Location,
            Description = request.Description,
            RequestDeadline = request.RequestDeadline.HasValue
                ? DateTime.SpecifyKind(request.RequestDeadline.Value, DateTimeKind.Utc)
                : null,
            Status = MatchStatus.Open,
            CreatedByAdminId = adminId
        };

        foreach (var slot in slots)
        {
            match.Slots.Add(new MatchSlot
            {
                Id = Guid.NewGuid(),
                StaffType = slot.Type,
                Needed = slot.Needed
            });
        }

        _db.Matches.Add(match);
        await _db.SaveChangesAsync();

        // Notify active staff whose role this match is asking for
        var neededTypes = match.Slots.Select(s => (StaffType?)s.StaffType).ToList();

        var recipientIds = await _db.Users
            .Where(u => u.Role == UserRole.Staff
                     && u.AccountStatus == AccountStatus.Active
                     && neededTypes.Contains(u.StaffType))
            .Select(u => u.Id)
            .ToListAsync();

        foreach (var userId in recipientIds)
        {
            await _notificationService.CreateAsync(userId, NotificationType.NewMatch,
                $"A new match \"{match.Title}\" was added on {match.MatchDate:MMM d} at {match.Location}.");
        }

        return ToDto(match);
    }

    public async Task<MatchDto?> UpdateAsync(Guid id, UpdateMatchRequest request)
    {
        var match = await _db.Matches
            .Include(m => m.Slots)
            .Include(m => m.Requests)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (match is null) return null;

        var newSlots = ParseSlots(request.Slots);

        // A role can't go below the people already accepted for it, and can't be removed while it has accepted people
        foreach (var slot in newSlots)
        {
            var accepted = match.AcceptedCountFor(slot.Type);
            if (slot.Needed < accepted)
                throw new InvalidOperationException(
                    $"{slot.Type}: {accepted} already accepted, so the needed number can't be lower than that.");
        }

        foreach (var existing in match.Slots)
        {
            if (newSlots.All(s => s.Type != existing.StaffType) && match.AcceptedCountFor(existing.StaffType) > 0)
                throw new InvalidOperationException(
                    $"{existing.StaffType} can't be removed because people are already accepted for it.");
        }

        match.Title = request.Title;
        match.TeamA = request.TeamA;
        match.TeamB = request.TeamB;
        match.Category = request.Category;
        match.MatchDate = DateTime.SpecifyKind(request.MatchDate, DateTimeKind.Utc);
        match.Location = request.Location;
        match.Description = request.Description;
        match.RequestDeadline = request.RequestDeadline.HasValue
            ? DateTime.SpecifyKind(request.RequestDeadline.Value, DateTimeKind.Utc)
            : null;

        // Sync the role slots
        var wanted = newSlots.ToDictionary(s => s.Type, s => s.Needed);

        foreach (var existing in match.Slots.ToList())
        {
            if (wanted.TryGetValue(existing.StaffType, out var needed))
                existing.Needed = needed;
            else
                _db.MatchSlots.Remove(existing);
        }

        foreach (var (type, needed) in wanted)
        {
            if (match.Slots.All(s => s.StaffType != type))
            {
                _db.MatchSlots.Add(new MatchSlot
                {
                    Id = Guid.NewGuid(),
                    MatchId = match.Id,
                    StaffType = type,
                    Needed = needed
                });
            }
        }

        // The match is closed only when every wanted role is full
        var allFilled = wanted.All(w => match.AcceptedCountFor(w.Key) >= w.Value);
        match.Status = allFilled ? MatchStatus.Closed : MatchStatus.Open;

        await _db.SaveChangesAsync();

        // Notify staff who have an active (pending or accepted) request on this match
        var involvedIds = match.Requests
            .Where(r => r.Status is RequestStatus.Pending or RequestStatus.Accepted)
            .Select(r => r.PhotographerId)
            .Distinct()
            .ToList();

        foreach (var userId in involvedIds)
        {
            await _notificationService.CreateAsync(userId, NotificationType.MatchUpdated,
                $"\"{match.Title}\" was updated. New date: {match.MatchDate:MMM d, h:mm tt}, location: {match.Location}.");
        }

        var refreshed = await _db.Matches
            .Include(m => m.Slots)
            .Include(m => m.Requests)
            .FirstAsync(m => m.Id == id);

        return ToDto(refreshed);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var match = await _db.Matches.Include(m => m.Requests)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (match is null) return false;

        var involvedIds = match.Requests
            .Where(r => r.Status is RequestStatus.Pending or RequestStatus.Accepted)
            .Select(r => r.PhotographerId)
            .Distinct()
            .ToList();

        var matchTitle = match.Title;

        _db.Matches.Remove(match);
        await _db.SaveChangesAsync();

        foreach (var userId in involvedIds)
        {
            await _notificationService.CreateAsync(userId, NotificationType.MatchCancelled,
                $"\"{matchTitle}\" was cancelled by the admin.");
        }

        return true;
    }

    private static List<(StaffType Type, int Needed)> ParseSlots(List<MatchSlotInput>? input)
    {
        var result = new List<(StaffType Type, int Needed)>();

        foreach (var item in input ?? new List<MatchSlotInput>())
        {
            if (item.Needed <= 0) continue; // a role with 0 people simply isn't requested

            if (!Enum.TryParse<StaffType>(item.StaffType, true, out var type) || !Enum.IsDefined(type))
                throw new InvalidOperationException($"Unknown role type: {item.StaffType}");

            if (result.Any(r => r.Type == type))
                throw new InvalidOperationException($"{type} is listed more than once.");

            result.Add((type, item.Needed));
        }

        if (result.Count == 0)
            throw new InvalidOperationException("The match needs at least one role with one or more people.");

        return result;
    }

    private static MatchDto ToDto(Match m)
    {
        var slots = m.Slots
            .OrderBy(s => s.StaffType)
            .Select(s => new MatchSlotDto
            {
                StaffType = s.StaffType.ToString(),
                Needed = s.Needed,
                Accepted = m.AcceptedCountFor(s.StaffType)
            })
            .ToList();

        return new MatchDto
        {
            Id = m.Id,
            Title = m.Title,
            TeamA = m.TeamA,
            TeamB = m.TeamB,
            Category = m.Category,
            MatchDate = m.MatchDate,
            Location = m.Location,
            Description = m.Description,
            Slots = slots,
            NeededTotal = slots.Sum(s => s.Needed),
            AcceptedTotal = slots.Sum(s => s.Accepted),
            RequestDeadline = m.RequestDeadline,
            Status = m.Status.ToString()
        };
    }
}