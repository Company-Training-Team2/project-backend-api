using EventHub.Domain.Enums;

namespace EventHub.Application.DTOs.Notification;

/// <summary>
/// Audit Module 10: payload used internally by other services (booking status
/// changes, vendor matches, payment receipts, reviews, messages, security
/// alerts) to raise a notification through INotificationService.NotifyAsync.
/// </summary>
public class CreateNotificationDto
{
    public int UserId { get; set; }

    public NotificationType Type { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public int? RelatedEntityId { get; set; }
}
