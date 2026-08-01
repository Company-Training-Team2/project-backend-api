using EventHub.Domain.Common;
using EventHub.Domain.Enums;

namespace EventHub.Domain.Entities;

public class Booking : AuditableEntity
{
    public int EventId { get; set; }

    public int WorkPostId { get; set; }

    public BookingStatus Status { get; set; }

    public decimal TotalPrice { get; set; }

    public int Quantity { get; set; }

    public string? Notes { get; set; }

    // Navigation Properties
    public Event Event { get; set; } = null!;

    public WorkPost WorkPost { get; set; } = null!;

    public Payment? Payment { get; set; }

    public Review? Review { get; set; }
}