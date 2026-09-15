using Microsoft.EntityFrameworkCore;
using ShotCall.Application.DTOs;
using ShotCall.Application.Interfaces;
using ShotCall.Domain.Entities;
using ShotCall.Domain.Enums;
using ShotCall.Infrastructure.Persistence;

namespace ShotCall.Infrastructure.Services;

public class CancellationRequestService : ICancellationRequestService
{
    private const int LateCancellationThresholdHours = 48;

    private readonly AppDbContext _db;
    private readonly INotificationService _notificationService;

    public CancellationRequestService(AppDbContext db, INotificationService notificationService)
    {
        _db = db;
        _notificationService = notificationService;
    }

    public async Task<List<CancellationRequestDto>> GetPendingAsync()
    {
        var items = await _db.CancellationRequests
            .Include(c => c.MatchRequest).ThenInclude(r => r.Match)
            .Include(c => c.MatchRequest).ThenInclude(r => r.Photographer)
            .Where(c => c.Status == CancellationStatus.Pending)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();

        return items.Select(ToDto).ToList();
    }

    public async Task<CancellationRequestDto> CreateAsync(CreateCancellationRequestDto request, Guid photographerId)
    {
        var matchRequest = await _db.MatchRequests
            .Include(r => r.Match)
            .Include(r => r.Photographer)
            .FirstOrDefaultAsync(r => r.Id == request.MatchRequestId)
            ?? throw new InvalidOperationException("Match request not found.");

        if (matchRequest.PhotographerId != photographerId)
            throw new InvalidOperationException("You can only cancel your own requests.");

        if (matchRequest.Status != RequestStatus.Accepted)
            throw new InvalidOperationException("Only accepted requests can be cancelled.");

        var existing = await _db.CancellationRequests
            .AnyAsync(c => c.MatchRequestId == request.MatchRequestId && c.Status == CancellationStatus.Pending);
        if (existing)
            throw new InvalidOperationException("A cancellation request is already pending for this match.");

        var isLate = (matchRequest.Match.MatchDate - DateTime.UtcNow).TotalHours < LateCancellationThresholdHours;

        var cancellation = new CancellationRequest
        {
            Id = Guid.NewGuid(),
            MatchRequestId = request.MatchRequestId,
            Reason = request.Reason,
            IsLateCancellation = isLate,
            Status = CancellationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _db.CancellationRequests.Add(cancellation);
        await _db.SaveChangesAsync();

        await _db.Entry(cancellation).Reference(c => c.MatchRequest).LoadAsync();

        await NotifyAdminsAsync(NotificationType.CancellationSubmitted,
            $"{matchRequest.Photographer.FullName} requested to cancel \"{matchRequest.Match.Title}\".");

        return ToDto(cancellation, matchRequest.Match, matchRequest.Photographer);
    }

    public async Task<CancellationRequestDto?> ApproveAsync(Guid id)
    {
        var cancellation = await _db.CancellationRequests
            .Include(c => c.MatchRequest).ThenInclude(r => r.Match).ThenInclude(m => m.WaitingList)
            .Include(c => c.MatchRequest).ThenInclude(r => r.Photographer)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cancellation is null) return null;
        if (cancellation.Status != CancellationStatus.Pending)
            throw new InvalidOperationException("This cancellation was already resolved.");

        cancellation.Status = CancellationStatus.Approved;
        cancellation.RespondedAt = DateTime.UtcNow;

        var matchRequest = cancellation.MatchRequest;
        var match = matchRequest.Match;

        matchRequest.Status = RequestStatus.Withdrawn;

        if (cancellation.IsLateCancellation)
        {
            _db.Violations.Add(new Violation
            {
                Id = Guid.NewGuid(),
                PhotographerId = matchRequest.PhotographerId,
                MatchId = match.Id,
                Type = ViolationType.LateCancellation,
                Notes = "Auto-logged: cancellation within the late-cancellation threshold.",
                CreatedAt = DateTime.UtcNow
            });
        }

        var next = match.WaitingList.OrderBy(w => w.Position).FirstOrDefault();
        Guid? promotedPhotographerId = null;
        if (next is not null)
        {
            _db.MatchRequests.Add(new MatchRequest
            {
                Id = Guid.NewGuid(),
                MatchId = match.Id,
                PhotographerId = next.PhotographerId,
                Status = RequestStatus.Accepted,
                RequestedAt = next.CreatedAt,
                RespondedAt = DateTime.UtcNow
            });
            _db.WaitingListEntries.Remove(next);
            promotedPhotographerId = next.PhotographerId;
        }
        else
        {
            match.Status = MatchStatus.Open;
        }

        await _db.SaveChangesAsync();

        await _notificationService.CreateAsync(matchRequest.PhotographerId, NotificationType.CancellationDecision,
            $"Your cancellation for \"{match.Title}\" was approved.");

        if (promotedPhotographerId.HasValue)
        {
            await _notificationService.CreateAsync(promotedPhotographerId.Value, NotificationType.RequestAccepted,
                $"You've been promoted from the waiting list for \"{match.Title}\".");
        }

        return ToDto(cancellation, match, matchRequest.Photographer);
    }

    public async Task<CancellationRequestDto?> RejectAsync(Guid id)
    {
        var cancellation = await _db.CancellationRequests
            .Include(c => c.MatchRequest).ThenInclude(r => r.Match)
            .Include(c => c.MatchRequest).ThenInclude(r => r.Photographer)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (cancellation is null) return null;
        if (cancellation.Status != CancellationStatus.Pending)
            throw new InvalidOperationException("This cancellation was already resolved.");

        cancellation.Status = CancellationStatus.Rejected;
        cancellation.RespondedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        await _notificationService.CreateAsync(cancellation.MatchRequest.PhotographerId, NotificationType.CancellationDecision,
            $"Your cancellation for \"{cancellation.MatchRequest.Match.Title}\" was rejected.");

        return ToDto(cancellation, cancellation.MatchRequest.Match, cancellation.MatchRequest.Photographer);
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

    private static CancellationRequestDto ToDto(CancellationRequest c, Match match, ApplicationUser photographer) => new()
    {
        Id = c.Id,
        MatchRequestId = c.MatchRequestId,
        MatchTitle = match.Title,
        PhotographerName = photographer.FullName,
        Reason = c.Reason,
        IsLateCancellation = c.IsLateCancellation,
        Status = c.Status.ToString(),
        CreatedAt = c.CreatedAt,
        RespondedAt = c.RespondedAt
    };

    private static CancellationRequestDto ToDto(CancellationRequest c) => ToDto(c, c.MatchRequest.Match, c.MatchRequest.Photographer);
}