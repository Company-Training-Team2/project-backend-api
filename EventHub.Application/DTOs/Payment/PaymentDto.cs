using EventHub.Domain.Enums;

namespace EventHub.Application.DTOs.Payment;

public class PaymentDto
{
    public int Id { get; set; }

    public int BookingId { get; set; }

    public decimal GrossAmount { get; set; }

    public decimal CommissionRateSnapshot { get; set; }

    public decimal PlatformFeeAmount { get; set; }

    public decimal VendorPayoutAmount { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public PaymentStatus PaymentStatus { get; set; }

    public string? TransactionId { get; set; }

    public string? PaymentGateway { get; set; }

    public DateTime? PaidAt { get; set; }
}

/// <summary>Result of POST /api/payments/checkout/{bookingId}.</summary>
public class CheckoutResultDto
{
    public int PaymentId { get; set; }

    public int BookingId { get; set; }

    public decimal GrossAmount { get; set; }

    public decimal PlatformFeeAmount { get; set; }

    public decimal VendorPayoutAmount { get; set; }

    public PaymentStatus PaymentStatus { get; set; }

    /// <summary>Iframe/unified checkout URL the client should redirect the customer to.</summary>
    public string CheckoutUrl { get; set; } = string.Empty;
}
