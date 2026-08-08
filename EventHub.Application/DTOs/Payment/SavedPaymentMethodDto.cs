using EventHub.Domain.Enums;

namespace EventHub.Application.DTOs.Payment;

public class SavedPaymentMethodDto
{
    public int Id { get; set; }

    public PaymentMethod Type { get; set; }

    public string MaskedNumber { get; set; } = string.Empty;

    public string? CardHolderName { get; set; }

    public int? ExpiryMonth { get; set; }

    public int? ExpiryYear { get; set; }

    public bool IsDefault { get; set; }
}

/// <summary>
/// POST /api/payments/methods. GatewayToken/MaskedNumber are expected to
/// come from the payment gateway's client-side tokenization step (e.g.
/// Paymob's save-card flow) — this endpoint never accepts a raw card number.
/// </summary>
public class CreateSavedPaymentMethodRequest
{
    public PaymentMethod Type { get; set; }

    public string MaskedNumber { get; set; } = string.Empty;

    public string? CardHolderName { get; set; }

    public int? ExpiryMonth { get; set; }

    public int? ExpiryYear { get; set; }

    public string? GatewayToken { get; set; }

    public bool IsDefault { get; set; }
}
