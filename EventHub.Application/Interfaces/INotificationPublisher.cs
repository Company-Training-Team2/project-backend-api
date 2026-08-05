using EventHub.Application.DTOs.Notification;

namespace EventHub.Application.Interfaces;

/// <summary>
/// Audit Module 10: real-time delivery abstraction. Keeps the Application
/// layer transport-agnostic — the API layer supplies the actual implementation
/// (a SignalR Hub push) so notifications reach connected clients without polling.
/// </summary>
public interface INotificationPublisher
{
    Task PublishAsync(int userId, NotificationDto notification);
}
