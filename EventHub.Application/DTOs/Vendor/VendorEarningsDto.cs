using EventHub.Domain.Enums;

namespace EventHub.Application.DTOs.Vendor;

/// <summary>GET /api/vendor/earnings — payment-module view of a vendor's PAID bookings and payout status.</summary>
public class VendorEarningsDto
{
    public decimal TotalEarnings { get; set; }

    public decimal PendingPayoutAmount { get; set; }

    public decimal ProcessedPayoutAmount { get; set; }

    public List<VendorEarningItemDto> Items { get; set; } = new();
}

public class VendorEarningItemDto
{
    public int BookingId { get; set; }

    public int PaymentId { get; set; }

    public decimal GrossAmount { get; set; }

    public decimal PlatformFeeAmount { get; set; }

    public decimal VendorPayoutAmount { get; set; }

    public PaymentStatus PaymentStatus { get; set; }

    public DateTime? PaidAt { get; set; }
}

/// <summary>GET /api/vendor/payouts.</summary>
public class PayoutDto
{
    public int Id { get; set; }

    public int PaymentId { get; set; }

    public int BookingId { get; set; }

    public decimal Amount { get; set; }

    public PayoutStatus Status { get; set; }

    public DateTime? ProcessedAt { get; set; }
}
