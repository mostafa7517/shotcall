using ShotCall.Application.DTOs;
using ShotCall.Domain.Enums;

namespace ShotCall.Application.Interfaces;

public interface INotificationService
{
    Task<List<NotificationDto>> GetForUserAsync(Guid userId);
    Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId);

    // Used internally by other services (requests, cancellations, accounts...) to push a notification
    Task CreateAsync(Guid userId, NotificationType type, string message);
}