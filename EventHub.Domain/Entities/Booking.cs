using EventHub.Domain.Common;
using EventHub.Domain.Enums;

namespace EventHub.Domain.Entities;

/// <summary>
/// Booking of a vendor WorkPost for a customer Event.
/// Status aligned to PRD flow: Pending → Accepted → Paid → Completed.
/// CustomerId references CustomerProfile.Id (not User.Id).
/// </summary>
public class Booking : AuditableEntity
{
    public int CustomerId { get; set; }

    public int EventId { get; set; }

    public int WorkPostId { get; set; }

    public DateOnly BookingDate { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    public decimal TotalPrice { get; set; }

    public int Quantity { get; set; }

    public string? Notes { get; set; }

    // ─── Navigation Properties ────────────────────────────────────────────────
    public CustomerProfile Customer { get; set; } = null!;

    public Event Event { get; set; } = null!;

    public WorkPost WorkPost { get; set; } = null!;

    public Payment? Payment { get; set; }

    public Review? Review { get; set; }

    /// <summary>Auto-generated expense entry created when booking is confirmed.</summary>
    public Expense? Expense { get; set; }
}