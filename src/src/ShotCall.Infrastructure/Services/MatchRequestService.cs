using Microsoft.EntityFrameworkCore;
using ShotCall.Application.DTOs;
using ShotCall.Application.Interfaces;
using ShotCall.Domain.Entities;
using ShotCall.Domain.Enums;
using ShotCall.Infrastructure.Persistence;

namespace ShotCall.Infrastructure.Services;

public class MatchRequestService : IMatchRequestService
{
    private readonly AppDbContext _db;
    private readonly INotificationService _notificationService;

    public MatchRequestService(AppDbContext db, INotificationService notificationService)
    {
        _db = db;
        _notificationService = notificationService;
    }

    public async Task<List<MatchRequestDto>> GetAllAsync(MatchRequestFilter filter)
    {
        var query = _db.MatchRequests
            .Include(r => r.Match)
            .Include(r => r.Photographer)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Status) &&
            Enum.TryParse<RequestStatus>(filter.Status, true, out var status))
            query = query.Where(r => r.Status == status);

        if (filter.MatchId.HasValue)
            query = query.Where(r => r.MatchId == filter.MatchId);

        if (filter.PhotographerId.HasValue)
            query = query.Where(r => r.PhotographerId == filter.PhotographerId);

        var requests = await query.OrderByDescending(r => r.RequestedAt).ToListAsync();
        return requests.Select(ToDto).ToList();
    }

    public async Task<MatchRequestDto> CreateAsync(CreateMatchRequestDto request, Guid photographerId)
    {
        var user = await _db.Users.FindAsync(photographerId)
            ?? throw new InvalidOperationException("Account not found.");

        if (user.StaffType is null)
            throw new InvalidOperationException("Choose your role in your profile before requesting matches.");

        var type = user.StaffType.Value;

        var match = await _db.Matches
            .Include(m => m.Slots)
            .Include(m => m.Requests)
            .FirstOrDefaultAsync(m => m.Id == request.MatchId)
            ?? throw new InvalidOperationException("Match not found.");

        if (match.Status != MatchStatus.Open)
            throw new InvalidOperationException("This match is closed for requests.");

        var slot = match.Slots.FirstOrDefault(s => s.StaffType == type);
        if (slot is null)
            throw new InvalidOperationException($"This match isn't asking for your role ({type}).");

        if (match.AcceptedCountFor(type) >= slot.Needed)
            throw new InvalidOperationException("All spots for your role are filled. Join the waiting list instead.");

        var alreadyRequested = match.Requests.Any(r =>
            r.PhotographerId == photographerId &&
            r.Status is RequestStatus.Pending or RequestStatus.Accepted);

        if (alreadyRequested)
            throw new InvalidOperationException("You already have an active request for this match.");

        var matchRequest = new MatchRequest
        {
            Id = Guid.NewGuid(),
            MatchId = request.MatchId,
            PhotographerId = photographerId,
            StaffType = type,
            Status = RequestStatus.Pending,
            RequestedAt = DateTime.UtcNow
        };

        _db.MatchRequests.Add(matchRequest);
        await _db.SaveChangesAsync();

        await _db.Entry(matchRequest).Reference(r => r.Match).LoadAsync();
        await _db.Entry(matchRequest).Reference(r => r.Photographer).LoadAsync();

        await NotifyAdminsAsync(NotificationType.NewRequest,
            $"{matchRequest.Photographer.FullName} ({type}) requested to cover \"{match.Title}\".");

        return ToDto(matchRequest);
    }

    public async Task<MatchRequestDto?> WithdrawAsync(Guid requestId, Guid photographerId)
    {
        var request = await _db.MatchRequests
            .Include(r => r.Match)
            .Include(r => r.Photographer)
            .FirstOrDefaultAsync(r => r.Id == requestId && r.PhotographerId == photographerId);

        if (request is null) return null;
        if (request.Status != RequestStatus.Pending)
            throw new InvalidOperationException("Only pending requests can be withdrawn.");

        request.Status = RequestStatus.Withdrawn;
        request.RespondedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return ToDto(request);
    }

    public async Task<MatchRequestDto?> AcceptAsync(Guid requestId)
    {
        var request = await _db.MatchRequests
            .Include(r => r.Match).ThenInclude(m => m.Slots)
            .Include(r => r.Match).ThenInclude(m => m.Requests)
            .Include(r => r.Photographer)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request is null) return null;
        if (request.Status != RequestStatus.Pending)
            throw new InvalidOperationException("Only pending requests can be accepted.");

        var match = request.Match;
        var slot = match.Slots.FirstOrDefault(s => s.StaffType == request.StaffType);

        if (slot is null || match.AcceptedCountFor(request.StaffType) >= slot.Needed)
            throw new InvalidOperationException($"All {request.StaffType} spots for this match are already filled.");

        request.Status = RequestStatus.Accepted;
        request.RespondedAt = DateTime.UtcNow;

        // The request is already part of match.Requests, so it is counted here
        if (match.IsFull())
            match.Status = MatchStatus.Closed;

        await _db.SaveChangesAsync();

        await _notificationService.CreateAsync(request.PhotographerId, NotificationType.RequestAccepted,
            $"Your request for \"{match.Title}\" was accepted.");

        return ToDto(request);
    }

    public async Task<MatchRequestDto?> RejectAsync(Guid requestId)
    {
        var request = await _db.MatchRequests
            .Include(r => r.Match)
            .Include(r => r.Photographer)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request is null) return null;
        if (request.Status != RequestStatus.Pending)
            throw new InvalidOperationException("Only pending requests can be rejected.");

        request.Status = RequestStatus.Rejected;
        request.RespondedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        await _notificationService.CreateAsync(request.PhotographerId, NotificationType.RequestRejected,
            $"Your request for \"{request.Match.Title}\" was rejected.");

        return ToDto(request);
    }

    private async Task NotifyAdminsAsync(NotificationType type, string message)
    {
        var adminIds = await _db.Users
            .Where(u => u.Role == UserRole.Admin)
            .Select(u => u.Id)
            .ToListAsync();

        foreach (var adminId in adminIds)
            await _notificationService.CreateAsync(adminId, type, message);
    }

    private static MatchRequestDto ToDto(MatchRequest r) => new()
    {
        Id = r.Id,
        MatchId = r.MatchId,
        MatchTitle = r.Match.Title,
        PhotographerId = r.PhotographerId,
        PhotographerName = r.Photographer.FullName,
        StaffType = r.StaffType.ToString(),
        Status = r.Status.ToString(),
        RequestedAt = r.RequestedAt,
        RespondedAt = r.RespondedAt
    };
}