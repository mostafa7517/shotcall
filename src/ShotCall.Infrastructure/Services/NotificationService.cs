using System.Net;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ShotCall.Application.DTOs;
using ShotCall.Application.Interfaces;
using ShotCall.Domain.Entities;
using ShotCall.Domain.Enums;
using ShotCall.Infrastructure.Hubs;
using ShotCall.Infrastructure.Persistence;

namespace ShotCall.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private const int MaxMessageLength = 500; // matches the database column

    private readonly AppDbContext _db;
    private readonly IHubContext<NotificationHub> _hub;
    private readonly IEmailService _emailService;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        AppDbContext db,
        IHubContext<NotificationHub> hub,
        IEmailService emailService,
        ILogger<NotificationService> logger)
    {
        _db = db;
        _hub = hub;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<List<NotificationDto>> GetForUserAsync(Guid userId)
    {
        var items = await _db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();

        return items.Select(n => new NotificationDto
        {
            Id = n.Id,
            Type = n.Type.ToString(),
            Message = n.Message,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        }).ToList();
    }

    public async Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId)
    {
        var notification = await _db.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);
        if (notification is null) return false;

        notification.IsRead = true;
        await _db.SaveChangesAsync();
        return true;
    }

    public Task CreateAsync(Guid userId, NotificationType type, string message) =>
        CreateAsync(userId, type, message, "ShotCall Notification", $"<p>{WebUtility.HtmlEncode(message)}</p>");

    public async Task CreateAsync(Guid userId, NotificationType type, string message, string emailSubject, string emailBodyHtml)
    {
        if (message.Length > MaxMessageLength)
            message = message[..(MaxMessageLength - 3)] + "...";

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Type = type,
            Message = message,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.Notifications.Add(notification);
        await _db.SaveChangesAsync();

        // Push it in real-time to the user's group (keyed by their UserId)
        await _hub.Clients.Group(userId.ToString()).SendAsync("ReceiveNotification", new NotificationDto
        {
            Id = notification.Id,
            Type = notification.Type.ToString(),
            Message = notification.Message,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt
        });

        // Also send an email as an addition to the in-app notification (not a replacement).
        // A mail problem (Gmail not connected, token revoked...) must never break the action
        // that triggered the notification, so it is only logged.
        try
        {
            var user = await _db.Users.FindAsync(userId);
            if (user is not null)
                await _emailService.SendAsync(user.Email, emailSubject, emailBodyHtml);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not send the email for notification {NotificationId}", notification.Id);
        }
    }
}
