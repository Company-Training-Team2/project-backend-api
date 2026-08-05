using EventHub.Domain.Common;
using EventHub.Domain.Enums;

namespace EventHub.Domain.Entities;

/// <summary>
/// Explicit expense ledger entry attached to an event.
/// Per audit Module 4: Expense (Id, EventId, Category, Description, Amount,
/// Status [Paid/Pending/Flagged], Date, BookingId?).
/// Hybrid model: system-generated entries from confirmed bookings + manual entries.
/// </summary>
public class Expense : AuditableEntity
{
    public int EventId { get; set; }

    public string Category { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public ExpenseStatus Status { get; set; } = ExpenseStatus.Pending;

    public DateTime Date { get; set; }

    /// <summary>
    /// Nullable FK — set when expense is auto-generated from a confirmed booking.
    /// Null for manually entered out-of-pocket expenses.
    /// </summary>
    public int? BookingId { get; set; }

    // ─── Navigation ───────────────────────────────────────────────────────────
    public Event Event { get; set; } = null!;

    public Booking? Booking { get; set; }
}