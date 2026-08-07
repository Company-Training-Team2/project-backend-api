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
    public string? TransactionId { get; set; }

    public string? PaymentGateway { get; set; }

    public Booking Booking { get; set; } = null!;

    // ─── Commission snapshot (Payment module) ─────────────────────────────────
    // Captured at checkout time so a later change to the platform commission
    // policy never affects amounts already charged/paid.

    /// <summary>Full price charged to the customer (== Booking.TotalPrice at checkout time).</summary>
    public decimal GrossAmount { get; set; }

    /// <summary>Platform commission rate snapshot (e.g. 0.10 for 10%) at the time this Payment was created.</summary>
    public decimal CommissionRateSnapshot { get; set; }

    /// <summary>GrossAmount * CommissionRateSnapshot.</summary>
    public decimal PlatformFeeAmount { get; set; }

    /// <summary>GrossAmount - PlatformFeeAmount — the amount owed to the vendor, paid out after event completion.</summary>
    public decimal VendorPayoutAmount { get; set; }

    public Payout? Payout { get; set; }
}
