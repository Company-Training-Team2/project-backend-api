using EventHub.Domain.Common;
using EventHub.Domain.Enums;

namespace EventHub.Domain.Entities;

/// <summary>
/// A single task in an event's planning checklist.
/// Per audit Module 5: ChecklistItem (Id, EventId, Title, Description,
/// DueDate, Priority, IsCompleted, Category).
/// </summary>
public class ChecklistItem : AuditableEntity
{
    public int EventId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    public bool IsCompleted { get; set; } = false;

    /// <summary>Vendor category link (e.g. "Catering", "Photography").</summary>
    public string? Category { get; set; }

    // ─── Navigation ───────────────────────────────────────────────────────────
    public Event Event { get; set; } = null!;
}