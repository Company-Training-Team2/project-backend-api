using EventHub.Domain.Common;
using EventHub.Domain.Enums;

namespace EventHub.Domain.Entities;

public class Payment : BaseEntity
{
    public int BookingId { get; set; }

    public decimal Amount { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public PaymentStatus PaymentStatus { get; set; }

    public DateTime? PaidAt { get; set; }

    // Navigation Property
    public Booking Booking { get; set; } = null!;
}