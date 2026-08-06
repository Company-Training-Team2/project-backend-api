using EventHub.Domain.Common;
using EventHub.Domain.Enums;

namespace EventHub.Domain.Entities;

/// <summary>
/// Represents a customer's event (wedding, birthday, etc.).
/// City / Location added per audit Module 3 schema update.
/// Guest sub-domain added via Guest entity (separate file).
/// </summary>
public class Event : SoftDeletableEntity
{
    public int CustomerId { get; set; }

    public string Name { get; set; } = string.Empty;

    public EventType EventType { get; set; }

    public DateTime TargetDate { get; set; }


    public int GuestCount { get; set; }

    public decimal TotalBudget { get; set; }

    /// <summary>City column added per audit Module 3.</summary>
    public string City { get; set; } = string.Empty;

    /// <summary>Location / venue column added per audit Module 3.</summary>
    public string Location { get; set; } = string.Empty;

    public string? Notes { get; set; }

    // ─── Navigation Properties ────────────────────────────────────────────────
    public CustomerProfile Customer { get; set; } = null!;

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    /// <summary>Guest RSVP list — new sub-domain per audit Module 3.</summary>
    public ICollection<Guest> Guests { get; set; } = new List<Guest>();

    /// <summary>Checklist tasks — new sub-domain per audit Module 5.</summary>
    public ICollection<ChecklistItem> ChecklistItems { get; set; } = new List<ChecklistItem>();

    /// <summary>Expense ledger — new sub-domain per audit Module 4.</summary>
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();

    /// <summary>Attached documents — new sub-domain per audit Module 7.</summary>
    public ICollection<Document> Documents { get; set; } = new List<Document>();
}