using EventHub.API.Hubs;
using EventHub.Application.DTOs.Notification;
using EventHub.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace EventHub.API.RealTime;

/// <summary>
/// Audit Module 10: pushes a persisted notification to the owning user's
/// connected clients over the NotificationHub, fulfilling the "push updates
/// without polling" requirement.
/// </summary>
public class SignalRNotificationPublisher : INotificationPublisher
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public SignalRNotificationPublisher(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public Task PublishAsync(int userId, NotificationDto notification)
    {
        return _hubContext.Clients
            .User(userId.ToString())
            .SendAsync("ReceiveNotification", notification);
    }
}
