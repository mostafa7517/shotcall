using System.Globalization;
using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ShotCall.Application.DTOs;
using ShotCall.Application.Interfaces;
using ShotCall.Domain.Entities;
using ShotCall.Domain.Enums;
using ShotCall.Infrastructure.Persistence;

namespace ShotCall.Infrastructure.Services;

public class MatchRequestService : IMatchRequestService
{
    // Two bookings closer than this are shown to the admin as a possible clash
    private static readonly TimeSpan ConflictWindow = TimeSpan.FromHours(6);

    private readonly AppDbContext _db;
    private readonly INotificationService _notificationService;
    private readonly IConfiguration? _config;

    public MatchRequestService(AppDbContext db, INotificationService notificationService, IConfiguration? config = null)
    {
        _db = db;
        _notificationService = notificationService;
        _config = config;
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

        var match = await LoadMatchForChangeAsync(request.MatchId)
            ?? throw new InvalidOperationException("Match not found.");

        if (match.Status != MatchStatus.Open)
            throw new InvalidOperationException("This match is closed for requests.");

        var slot = match.Slots.FirstOrDefault(s => s.StaffType == type);
        if (slot is null)
            throw new InvalidOperationException($"This match isn't asking for your role ({type}).");

        // Spots held by pending invitations count too, so nobody gets squeezed out later
        if (match.ReservedCountFor(type) >= slot.Needed)
            throw new InvalidOperationException("All spots for your role are filled. Join the waiting list instead.");

        var alreadyActive = match.Requests.Any(r =>
            r.PhotographerId == photographerId &&
            r.Status is RequestStatus.Pending or RequestStatus.Accepted or RequestStatus.Invited);

        if (alreadyActive)
            throw new InvalidOperationException("You already have an active request or invitation for this match.");

        var matchRequest = UpsertRequest(match, photographerId, type, RequestStatus.Pending, null);

        await SaveAsync();

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

        // The pending request itself isn't counted yet, so this is the number of spots already taken
        if (slot is null || match.ReservedCountFor(request.StaffType) >= slot.Needed)
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

    // ---------------------------------------------------------------------
    // Manual appointments (admin invites people, they confirm or decline)
    // ---------------------------------------------------------------------

    public async Task<List<AssignCandidateDto>> GetCandidatesAsync(Guid matchId)
    {
        var match = await _db.Matches
            .Include(m => m.Slots)
            .Include(m => m.Requests)
            .FirstOrDefaultAsync(m => m.Id == matchId)
            ?? throw new InvalidOperationException("Match not found.");

        var neededTypes = match.Slots.Select(s => (StaffType?)s.StaffType).ToList();

        var users = await _db.Users
            .Where(u => u.Role == UserRole.Staff
                     && u.AccountStatus == AccountStatus.Active
                     && neededTypes.Contains(u.StaffType))
            .OrderBy(u => u.StaffType).ThenBy(u => u.FullName)
            .ToListAsync();

        var userIds = users.Select(u => u.Id).ToList();
        var windowStart = match.MatchDate - ConflictWindow;
        var windowEnd = match.MatchDate + ConflictWindow;

        // Other matches these people are already booked for around the same time
        var others = await _db.MatchRequests
            .Include(r => r.Match)
            .Where(r => userIds.Contains(r.PhotographerId)
                     && r.MatchId != matchId
                     && (r.Status == RequestStatus.Accepted || r.Status == RequestStatus.Invited)
                     && r.Match.MatchDate >= windowStart
                     && r.Match.MatchDate <= windowEnd)
            .ToListAsync();

        var result = new List<AssignCandidateDto>();
        foreach (var user in users)
        {
            var current = match.Requests.FirstOrDefault(r => r.PhotographerId == user.Id);
            var hasActive = current is not null &&
                current.Status is RequestStatus.Pending or RequestStatus.Accepted or RequestStatus.Invited;

            result.Add(new AssignCandidateDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                ProfilePictureUrl = user.ProfilePictureUrl,
                StaffType = user.StaffType!.Value.ToString(),
                ReliabilityScore = user.ReliabilityScore,
                RequestStatus = hasActive ? current!.Status.ToString() : null,
                Conflicts = others
                    .Where(o => o.PhotographerId == user.Id)
                    .Select(o => o.Match.Title)
                    .Distinct()
                    .ToList()
            });
        }

        return result;
    }

    public async Task<List<MatchRequestDto>> InviteAsync(Guid matchId, List<Guid> userIds)
    {
        var ids = (userIds ?? new List<Guid>()).Distinct().ToList();
        if (ids.Count == 0)
            throw new InvalidOperationException("Pick at least one person.");

        var match = await LoadMatchForChangeAsync(matchId)
            ?? throw new InvalidOperationException("Match not found.");

        if (match.MatchDate < DateTime.UtcNow)
            throw new InvalidOperationException("This match has already happened.");

        var users = await _db.Users.Where(u => ids.Contains(u.Id)).ToListAsync();

        // Everything is checked before anything is saved, so a bad pick changes nothing.
        // Each invitation is counted as it is added, so a batch can't overfill a role either.
        var invited = new List<MatchRequest>();
        var accepted = new List<MatchRequest>();
        foreach (var id in ids)
        {
            var user = users.FirstOrDefault(u => u.Id == id)
                ?? throw new InvalidOperationException("One of the selected people was not found.");

            if (user.Role != UserRole.Staff || user.AccountStatus != AccountStatus.Active)
                throw new InvalidOperationException($"{user.FullName} is not an active staff account.");

            if (user.StaffType is null)
                throw new InvalidOperationException($"{user.FullName} hasn't chosen a role yet.");

            var type = user.StaffType.Value;

            var slot = match.Slots.FirstOrDefault(s => s.StaffType == type)
                ?? throw new InvalidOperationException($"This match isn't asking for {RoleName(type)} ({user.FullName}).");

            var current = match.Requests.FirstOrDefault(r => r.PhotographerId == id);
            if (current is not null && current.Status is RequestStatus.Accepted or RequestStatus.Invited)
                throw new InvalidOperationException($"{user.FullName} is already on this match.");

            if (match.ReservedCountFor(type) >= slot.Needed)
                throw new InvalidOperationException($"No free {RoleName(type)} spots left ({user.FullName}).");

            if (current is not null && current.Status == RequestStatus.Pending)
            {
                // They asked for this match themselves, so picking them simply accepts that request
                current.Status = RequestStatus.Accepted;
                current.RespondedAt = DateTime.UtcNow;
                accepted.Add(current);
            }
            else
            {
                invited.Add(UpsertRequest(match, id, type, RequestStatus.Invited, null));
            }

            // Appointed people leave the waiting list
            var waiting = match.WaitingList.FirstOrDefault(w => w.PhotographerId == id);
            if (waiting is not null)
                _db.WaitingListEntries.Remove(waiting);
        }

        if (match.IsFull())
            match.Status = MatchStatus.Closed;

        await SaveAsync();

        var touched = accepted.Concat(invited).ToList();
        foreach (var request in touched)
        {
            await _db.Entry(request).Reference(r => r.Match).LoadAsync();
            await _db.Entry(request).Reference(r => r.Photographer).LoadAsync();
        }

        foreach (var request in accepted)
        {
            await _notificationService.CreateAsync(request.PhotographerId, NotificationType.RequestAccepted,
                $"Your request for \"{match.Title}\" was accepted.");
        }

        foreach (var request in invited)
        {
            var message =
                $"You've been appointed as {RoleName(request.StaffType)} for \"{match.Title}\" ({match.Category}) " +
                $"on {FormatDate(match.MatchDate)} at {match.Location}. Please confirm or decline.";

            var (subject, body) = BuildInvitationEmail(match, request);

            await _notificationService.CreateAsync(request.PhotographerId, NotificationType.MatchInvitation,
                message, subject, body);
        }

        return touched.Select(ToDto).ToList();
    }

    public async Task<MatchRequestDto?> RevokeInvitationAsync(Guid requestId)
    {
        var request = await _db.MatchRequests
            .AsNoTracking()
            .Include(r => r.Match)
            .Include(r => r.Photographer)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request is null) return null;

        // Only one answer can win: if the person confirmed a moment ago, this changes nothing
        var changed = await _db.MatchRequests
            .Where(r => r.Id == requestId && r.Status == RequestStatus.Invited)
            .ExecuteUpdateAsync(s => s
                .SetProperty(r => r.Status, RequestStatus.Withdrawn)
                .SetProperty(r => r.RespondedAt, (DateTime?)DateTime.UtcNow));

        if (changed == 0)
            throw new InvalidOperationException("This invitation was already answered.");

        request.Status = RequestStatus.Withdrawn;

        await FreeSpotAsync(request.MatchId, request.StaffType);

        await _notificationService.CreateAsync(request.PhotographerId, NotificationType.InvitationRevoked,
            $"Your appointment for \"{request.Match.Title}\" was cancelled by the admin.");

        return ToDto(request);
    }

    public async Task<MatchRequestDto?> ConfirmInvitationAsync(Guid requestId, Guid userId)
    {
        var request = await _db.MatchRequests
            .AsNoTracking()
            .Include(r => r.Match)
            .Include(r => r.Photographer)
            .FirstOrDefaultAsync(r => r.Id == requestId && r.PhotographerId == userId);

        if (request is null) return null;

        var changed = await _db.MatchRequests
            .Where(r => r.Id == requestId && r.PhotographerId == userId && r.Status == RequestStatus.Invited)
            .ExecuteUpdateAsync(s => s
                .SetProperty(r => r.Status, RequestStatus.Accepted)
                .SetProperty(r => r.RespondedAt, (DateTime?)DateTime.UtcNow));

        if (changed == 0)
            throw new InvalidOperationException("This invitation is no longer waiting for your answer.");

        request.Status = RequestStatus.Accepted;

        await NotifyAdminsAsync(NotificationType.InvitationConfirmed,
            $"{request.Photographer.FullName} ({RoleName(request.StaffType)}) confirmed \"{request.Match.Title}\".");

        return ToDto(request);
    }

    public async Task<MatchRequestDto?> DeclineInvitationAsync(Guid requestId, Guid userId, string? reason)
    {
        var request = await _db.MatchRequests
            .AsNoTracking()
            .Include(r => r.Match)
            .Include(r => r.Photographer)
            .FirstOrDefaultAsync(r => r.Id == requestId && r.PhotographerId == userId);

        if (request is null) return null;

        var changed = await _db.MatchRequests
            .Where(r => r.Id == requestId && r.PhotographerId == userId && r.Status == RequestStatus.Invited)
            .ExecuteUpdateAsync(s => s
                .SetProperty(r => r.Status, RequestStatus.Declined)
                .SetProperty(r => r.RespondedAt, (DateTime?)DateTime.UtcNow));

        if (changed == 0)
            throw new InvalidOperationException("This invitation is no longer waiting for your answer.");

        request.Status = RequestStatus.Declined;

        await FreeSpotAsync(request.MatchId, request.StaffType);

        var why = string.IsNullOrWhiteSpace(reason) ? "" : $" Reason: {reason.Trim()}";
        await NotifyAdminsAsync(NotificationType.InvitationDeclined,
            $"{request.Photographer.FullName} ({RoleName(request.StaffType)}) declined \"{request.Match.Title}\".{why}");

        return ToDto(request);
    }

    // ---------------------------------------------------------------------
    // Helpers
    // ---------------------------------------------------------------------

    private Task<Match?> LoadMatchForChangeAsync(Guid matchId) =>
        _db.Matches
            .Include(m => m.Slots)
            .Include(m => m.WaitingList)
            .Include(m => m.Requests).ThenInclude(r => r.CancellationRequest)
            .FirstOrDefaultAsync(m => m.Id == matchId);

    // A person can only have one request row per match (unique index), so an old
    // withdrawn / rejected / declined row is reused instead of inserting a second one.
    private MatchRequest UpsertRequest(Match match, Guid userId, StaffType type, RequestStatus status, DateTime? respondedAt)
    {
        var existing = match.Requests.FirstOrDefault(r => r.PhotographerId == userId);
        if (existing is not null)
        {
            // A cancellation belongs to the old row (one per row), so it goes with the old story
            if (existing.CancellationRequest is not null)
                _db.CancellationRequests.Remove(existing.CancellationRequest);

            existing.StaffType = type;
            existing.Status = status;
            existing.RequestedAt = DateTime.UtcNow;
            existing.RespondedAt = respondedAt;
            return existing;
        }

        var created = new MatchRequest
        {
            Id = Guid.NewGuid(),
            MatchId = match.Id,
            PhotographerId = userId,
            StaffType = type,
            Status = status,
            RequestedAt = DateTime.UtcNow,
            RespondedAt = respondedAt
        };
        // Must go through the DbSet: with a key we set ourselves, EF would treat an entity added only to
        // match.Requests as already existing and send an UPDATE (0 rows -> concurrency error).
        // Add() also puts it into match.Requests, so capacity counting within a batch still sees it.
        _db.MatchRequests.Add(created);
        return created;
    }

    // A spot was released: the next person waiting for that role takes it, otherwise the match reopens
    private async Task FreeSpotAsync(Guid matchId, StaffType type)
    {
        var match = await LoadMatchForChangeAsync(matchId);
        if (match is null) return;

        Guid? promotedUserId = null;

        var slot = match.Slots.FirstOrDefault(s => s.StaffType == type);
        if (slot is not null && match.ReservedCountFor(type) < slot.Needed)
        {
            var next = match.WaitingList
                .Where(w => w.StaffType == type)
                .OrderBy(w => w.Position)
                .FirstOrDefault();

            if (next is not null)
            {
                UpsertRequest(match, next.PhotographerId, type, RequestStatus.Accepted, DateTime.UtcNow);
                _db.WaitingListEntries.Remove(next);
                promotedUserId = next.PhotographerId;
            }
        }

        match.Status = match.IsFull() ? MatchStatus.Closed : MatchStatus.Open;

        await SaveAsync();

        if (promotedUserId.HasValue)
        {
            await _notificationService.CreateAsync(promotedUserId.Value, NotificationType.RequestAccepted,
                $"You've been promoted from the waiting list for \"{match.Title}\".");
        }
    }

    private async Task SaveAsync()
    {
        try
        {
            await _db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException { SqlState: "23505" })
        {
            // unique index (match + person): someone else just created the same request
            throw new InvalidOperationException("This match was just changed by someone else. Please refresh and try again.");
        }
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

    private static string RoleName(StaffType type) => type == StaffType.Tv ? "TV" : type.ToString();

    // The invitation email: role, what, when, where (and the MC for an event) + a button to answer
    private (string Subject, string Html) BuildInvitationEmail(Match match, MatchRequest request)
    {
        var baseUrl = (_config?["Frontend:BaseUrl"] ?? "http://localhost:5173").TrimEnd('/');
        var kind = match.IsEvent ? "event" : "match";
        var firstName = request.Photographer.FullName
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault() ?? "there";

        static string Row(string label, string value) =>
            $"<tr><td style='padding:6px 16px 6px 0;color:#6b7280'>{WebUtility.HtmlEncode(label)}</td>" +
            $"<td style='padding:6px 0;font-weight:600'>{WebUtility.HtmlEncode(value)}</td></tr>";

        var rows =
            Row("Your role", RoleName(request.StaffType)) +
            Row(match.IsEvent ? "Event" : "Match", match.Title) +
            (!match.IsEvent && !string.IsNullOrWhiteSpace(match.TeamA) && !string.IsNullOrWhiteSpace(match.TeamB)
                ? Row("Teams", $"{match.TeamA} vs {match.TeamB}")
                : "") +
            Row("Category", match.Category) +
            Row("When", FormatDate(match.MatchDate)) +
            Row("Where", match.Location) +
            (match.IsEvent && !string.IsNullOrWhiteSpace(match.HostName)
                ? Row("MC / host", match.HostName!)
                : "");

        var html =
            "<div style='font-family:Segoe UI,Arial,sans-serif;max-width:520px;margin:0 auto;color:#14171f'>" +
            $"<h2 style='margin:0 0 12px'>You've been appointed to cover a {kind}</h2>" +
            $"<p>Hi {WebUtility.HtmlEncode(firstName)}, the admin picked you for this {kind}:</p>" +
            $"<table style='border-collapse:collapse;margin:12px 0'>{rows}</table>" +
            "<p>Please confirm or decline as soon as you can, so the spot isn't left hanging.</p>" +
            $"<p><a href='{WebUtility.HtmlEncode(baseUrl)}/my-requests' " +
            "style='display:inline-block;background:#2563eb;color:#ffffff;padding:10px 18px;border-radius:6px;" +
            "text-decoration:none;font-weight:600'>Review and answer</a></p>" +
            "<p style='color:#6b7280;font-size:12px'>You need to be logged in to ShotCall to answer.</p>" +
            "</div>";

        return ($"ShotCall: you've been appointed - {match.Title}", html);
    }

    // Matches are stored in UTC; people read them in Cairo time
    private static string FormatDate(DateTime utc)
    {
        var asUtc = DateTime.SpecifyKind(utc, DateTimeKind.Utc);

        foreach (var zoneId in new[] { "Africa/Cairo", "Egypt Standard Time" })
        {
            try
            {
                var zone = TimeZoneInfo.FindSystemTimeZoneById(zoneId);
                var local = TimeZoneInfo.ConvertTimeFromUtc(asUtc, zone);
                return local.ToString("ddd d MMM yyyy, h:mm tt", CultureInfo.InvariantCulture) + " (Cairo time)";
            }
            catch (TimeZoneNotFoundException) { }
            catch (InvalidTimeZoneException) { }
        }

        return asUtc.ToString("ddd d MMM yyyy, HH:mm", CultureInfo.InvariantCulture) + " UTC";
    }

    private static MatchRequestDto ToDto(MatchRequest r) => new()
    {
        Id = r.Id,
        MatchId = r.MatchId,
        MatchTitle = r.Match.Title,
        MatchDate = r.Match.MatchDate,
        MatchLocation = r.Match.Location,
        PhotographerId = r.PhotographerId,
        PhotographerName = r.Photographer.FullName,
        StaffType = r.StaffType.ToString(),
        Status = r.Status.ToString(),
        RequestedAt = r.RequestedAt,
        RespondedAt = r.RespondedAt
    };
}
