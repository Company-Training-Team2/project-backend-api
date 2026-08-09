using EventHub.Domain.Common;
using EventHub.Domain.Enums;

namespace EventHub.Domain.Entities;

/// <summary>
/// A customer's saved payment instrument for faster checkout.
/// Per audit Module 8: GET / POST / DELETE /api/payments/methods.
///
/// PCI-DSS note: only ever stores a masked display number and a gateway
/// token (Paymob card token) — never a raw PAN, CVV, or full card number.
/// The actual charge on checkout still goes through IPaymentGateway using
/// GatewayToken; this entity is a display + reference record only.
/// </summary>
public class SavedPaymentMethod : AuditableEntity
{
    public int CustomerId { get; set; }

    public PaymentMethod Type { get; set; }

    /// <summary>Masked display value, e.g. "•••• 4242".</summary>
    public string MaskedNumber { get; set; } = string.Empty;

    public string? CardHolderName { get; set; }

    public int? ExpiryMonth { get; set; }

    public int? ExpiryYear { get; set; }

    /// <summary>Gateway-issued token used to charge this instrument again — never a raw PAN.</summary>
    public string? GatewayToken { get; set; }

    public bool IsDefault { get; set; }

    // ─── Navigation Properties ────────────────────────────────────────────────
    public CustomerProfile Customer { get; set; } = null!;
}
