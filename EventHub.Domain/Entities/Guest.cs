using EventHub.Domain.Common;
using EventHub.Domain.Enums;

namespace EventHub.Domain.Entities;

/// <summary>
/// Represents a guest invited to an event.
/// Added per audit Module 3: "Guest entity (Id, EventId, Name, RSVPStatus)
/// to support the RSVP list widget (Confirmed 124 / Pending 42 / Declined 8)".
/// </summary>
public class Guest : AuditableEntity
{
    public int EventId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public RSVPStatus RSVPStatus { get; set; } = RSVPStatus.Pending;

    // ─── Navigation ───────────────────────────────────────────────────────────
    public Event Event { get; set; } = null!;
}