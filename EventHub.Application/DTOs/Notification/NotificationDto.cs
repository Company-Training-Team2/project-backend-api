using EventHub.Domain.Enums;

namespace EventHub.Application.DTOs.Notification;

/// <summary>Audit Module 10: single inbox item, and the shape pushed over SignalR.</summary>
public class NotificationDto
{
    public int Id { get; set; }

    public NotificationType Type { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public bool IsRead { get; set; }

    public int? RelatedEntityId { get; set; }

    public DateTime CreatedAt { get; set; }
}
