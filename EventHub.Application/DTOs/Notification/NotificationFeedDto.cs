namespace EventHub.Application.DTOs.Notification;

/// <summary>
/// Audit Module 10: GET /api/notifications response — feed grouped
/// chronologically (Today, Yesterday, Earlier), newest first within each bucket.
/// </summary>
public class NotificationFeedDto
{
    public List<NotificationDto> Today { get; set; } = new();

    public List<NotificationDto> Yesterday { get; set; } = new();

    public List<NotificationDto> Earlier { get; set; } = new();

    public int UnreadCount { get; set; }
}
