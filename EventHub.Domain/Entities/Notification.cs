using EventHub.Domain.Common;
using EventHub.Domain.Enums;

namespace EventHub.Domain.Entities;

/// <summary>
/// In-app notification for a user.
/// Per audit Module 10: Notification (Id, UserId, Type, Title, Body,
/// IsRead, CreatedAt, RelatedEntityId).
/// Dispatched by background jobs / event-driven publishing.
/// Real-time delivery via SignalR Hub.
/// </summary>
public class Notification : AuditableEntity
{
    public int UserId { get; set; }

    public NotificationType Type { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public bool IsRead { get; set; } = false;

    /// <summary>FK to the related entity (BookingId, EventId, etc.) for deep-linking.</summary>
    public int? RelatedEntityId { get; set; }

    // ─── Navigation ───────────────────────────────────────────────────────────
    public User User { get; set; } = null!;
}