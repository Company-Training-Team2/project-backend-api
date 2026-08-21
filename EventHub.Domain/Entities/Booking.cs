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

    /// <summary>
    /// Nullable — not because a booking can be created without an event
    /// (CreateBookingDto.EventId is still required), but because Event is
    /// soft-deletable and a customer can delete an Event with real,
    /// already-Accepted/Paid bookings attached (EventService.DeleteEventAsync
    /// has no such guard). A required FK here would make EF apply Event's
    /// soft-delete filter through the join whenever Event was navigated/
    /// queried on, silently dropping the Booking out of vendor/admin
    /// booking lists and payment history — real money and commitments
    /// disappearing along with a UI-only "delete". Nullable + optional
    /// navigation keeps the Booking (and its Payment/Expense) fully visible
    /// even after its Event is gone; EventId still always gets set at
    /// creation time, this only changes what happens to *reads* once the
    /// referenced Event is later soft-deleted.
    /// </summary>
    public int? EventId { get; set; }

    public int WorkPostId { get; set; }

    public DateOnly BookingDate { get; set; }

    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    public decimal TotalPrice { get; set; }

    public int Quantity { get; set; }

    public string? Notes { get; set; }

    // ─── Navigation Properties ────────────────────────────────────────────────
    public CustomerProfile Customer { get; set; } = null!;

    public Event? Event { get; set; }

    public WorkPost WorkPost { get; set; } = null!;

    public Payment? Payment { get; set; }

    public Review? Review { get; set; }

    /// <summary>Auto-generated expense entry created when booking is confirmed.</summary>
    public Expense? Expense { get; set; }
}