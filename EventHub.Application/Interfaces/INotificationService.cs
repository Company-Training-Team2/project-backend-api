using EventHub.Application.DTOs.Notification;

namespace EventHub.Application.Interfaces;

/// <summary>Audit Module 10 (Notifications).</summary>
public interface INotificationService
{
    /// <summary>
    /// Persists a notification and dispatches it in real time. This is the
    /// event-driven publishing entry point other services call when something
    /// notification-worthy happens (booking status update, vendor match,
    /// security alert, payment receipt, new review, new message).
    /// </summary>
    Task<NotificationDto> NotifyAsync(CreateNotificationDto dto);

    /// <summary>GET /api/notifications — structured feed grouped into Today / Yesterday / Earlier.</summary>
    Task<NotificationFeedDto> GetFeedAsync();

    /// <summary>PATCH /api/notifications/{id}/read — marks a specific notification as read.</summary>
    Task<NotificationDto> MarkAsReadAsync(int notificationId);
}
