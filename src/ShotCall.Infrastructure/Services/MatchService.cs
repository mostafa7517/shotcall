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
        var query = _db.Matches.Include(m => m.Requests).AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Category))
            query = query.Where(m => m.Category == filter.Category);

        if (!string.IsNullOrWhiteSpace(filter.Status) &&
            Enum.TryParse<MatchStatus>(filter.Status, true, out var status))
            query = query.Where(m => m.Status == status);

        var matches = await query.OrderBy(m => m.MatchDate).ToListAsync();
        return matches.Select(ToDto).ToList();
    }

    public async Task<MatchDto?> GetByIdAsync(Guid id)
    {
        var match = await _db.Matches.Include(m => m.Requests)
            .FirstOrDefaultAsync(m => m.Id == id);
        return match is null ? null : ToDto(match);
    }

    public async Task<MatchDto> CreateAsync(CreateMatchRequest request, Guid adminId)
    {
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
            PhotographersNeeded = request.PhotographersNeeded,
            RequestDeadline = request.RequestDeadline.HasValue
                ? DateTime.SpecifyKind(request.RequestDeadline.Value, DateTimeKind.Utc)
                : null,
            Status = MatchStatus.Open,
            CreatedByAdminId = adminId
        };

        _db.Matches.Add(match);
        await _db.SaveChangesAsync();

        // Broadcast: notify every active photographer that a new match is available
        var activePhotographerIds = await _db.Users
            .Where(u => u.Role == UserRole.Photographer && u.AccountStatus == AccountStatus.Active)
            .Select(u => u.Id)
            .ToListAsync();

        foreach (var photographerId in activePhotographerIds)
        {
            await _notificationService.CreateAsync(photographerId, NotificationType.NewMatch,
                $"A new match \"{match.Title}\" was added on {match.MatchDate:MMM d} at {match.Location}.");
        }

        return ToDto(match);
    }

    public async Task<MatchDto?> UpdateAsync(Guid id, UpdateMatchRequest request)
    {
        var match = await _db.Matches.Include(m => m.Requests)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (match is null) return null;

        match.Title = request.Title;
        match.TeamA = request.TeamA;
        match.TeamB = request.TeamB;
        match.Category = request.Category;
        match.MatchDate = DateTime.SpecifyKind(request.MatchDate, DateTimeKind.Utc);
        match.Location = request.Location;
        match.Description = request.Description;
        match.PhotographersNeeded = request.PhotographersNeeded;
        match.RequestDeadline = request.RequestDeadline.HasValue
            ? DateTime.SpecifyKind(request.RequestDeadline.Value, DateTimeKind.Utc)
            : null;

        await _db.SaveChangesAsync();

        // Notify photographers who have an active (pending or accepted) request on this match
        var involvedPhotographerIds = match.Requests
            .Where(r => r.Status is RequestStatus.Pending or RequestStatus.Accepted)
            .Select(r => r.PhotographerId)
            .Distinct()
            .ToList();

        foreach (var photographerId in involvedPhotographerIds)
        {
            await _notificationService.CreateAsync(photographerId, NotificationType.MatchUpdated,
        $"\"{match.Title}\" was updated. New date: {match.MatchDate:MMM d, h:mm tt}, location: {match.Location}.");
        }

        return ToDto(match);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var match = await _db.Matches.Include(m => m.Requests)
            .FirstOrDefaultAsync(m => m.Id == id);
        if (match is null) return false;

        var involvedPhotographerIds = match.Requests
            .Where(r => r.Status is RequestStatus.Pending or RequestStatus.Accepted)
            .Select(r => r.PhotographerId)
            .Distinct()
            .ToList();

        var matchTitle = match.Title;

        _db.Matches.Remove(match);
        await _db.SaveChangesAsync();

        foreach (var photographerId in involvedPhotographerIds)
        {
            await _notificationService.CreateAsync(photographerId, NotificationType.MatchCancelled,
        $"\"{matchTitle}\" was cancelled by the admin.");
        }

        return true;
    }

    private static MatchDto ToDto(Match m) => new()
    {
        Id = m.Id,
        Title = m.Title,
        TeamA = m.TeamA,
        TeamB = m.TeamB,
        Category = m.Category,
        MatchDate = m.MatchDate,
        Location = m.Location,
        Description = m.Description,
        PhotographersNeeded = m.PhotographersNeeded,
        AcceptedCount = m.Requests.Count(r => r.Status == RequestStatus.Accepted),
        RequestDeadline = m.RequestDeadline,
        Status = m.Status.ToString()
    };
}