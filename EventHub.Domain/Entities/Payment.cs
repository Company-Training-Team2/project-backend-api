using EventHub.Domain.Common;
using EventHub.Domain.Enums;
namespace EventHub.Domain.Entities;

public class Payment : AuditableEntity
{
    public int BookingId { get; set; }

    public decimal Amount { get; set; }

    public string PaymentMethod { get; set; } = string.Empty;

   public PaymentStatus PaymentStatus { get; set; }

    public DateTime? PaidAt { get; set; }

    public Booking Booking { get; set; } = null!;
}