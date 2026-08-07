using System.Text.Json.Serialization;

namespace EventHub.Application.DTOs.Payment;

/// <summary>Everything IPaymentGateway needs to open a checkout session for one Payment.</summary>
public class PaymentGatewayRequest
{
    public int BookingId { get; set; }

    public int PaymentId { get; set; }

    public decimal AmountEgp { get; set; }

    public string CustomerFirstName { get; set; } = string.Empty;

    public string CustomerLastName { get; set; } = string.Empty;

    public string CustomerEmail { get; set; } = string.Empty;

    public string? CustomerPhone { get; set; }
}

/// <summary>Result of IPaymentGateway.CreatePaymentKeyAsync.</summary>
public class PaymentGatewayResult
{
    public bool Success { get; set; }

    public string? PaymentToken { get; set; }

    public string? CheckoutUrl { get; set; }

    public long? GatewayOrderId { get; set; }

    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Raw envelope Paymob posts to the Transaction Processed Callback endpoint:
/// { "type": "TRANSACTION", "obj": { ... } }.
/// </summary>
public class PaymobWebhookEnvelope
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("obj")]
    public PaymobTransactionCallbackDto Obj { get; set; } = new();
}

/// <summary>
/// Fields of a Paymob transaction callback that matter for our domain logic and
/// for HMAC verification. Field names/casing and the exact HMAC field order are
/// per Paymob's official docs at the time this was written — re-verify against
/// https://developers.paymob.com before relying on this in production, since
/// payment gateway APIs can change without notice.
/// </summary>
public class PaymobTransactionCallbackDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("pending")]
    public bool Pending { get; set; }

    [JsonPropertyName("amount_cents")]
    public long AmountCents { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("is_auth")]
    public bool IsAuth { get; set; }

    [JsonPropertyName("is_capture")]
    public bool IsCapture { get; set; }

    [JsonPropertyName("is_standalone_payment")]
    public bool IsStandalonePayment { get; set; }

    [JsonPropertyName("is_voided")]
    public bool IsVoided { get; set; }

    [JsonPropertyName("is_refunded")]
    public bool IsRefunded { get; set; }

    [JsonPropertyName("is_3d_secure")]
    public bool Is3DSecure { get; set; }

    [JsonPropertyName("integration_id")]
    public long IntegrationId { get; set; }

    [JsonPropertyName("has_parent_transaction")]
    public bool HasParentTransaction { get; set; }

    [JsonPropertyName("order")]
    public PaymobOrderRefDto Order { get; set; } = new();

    [JsonPropertyName("created_at")]
    public string CreatedAt { get; set; } = string.Empty;

    [JsonPropertyName("currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonPropertyName("error_occured")]
    public bool ErrorOccured { get; set; }

    [JsonPropertyName("owner")]
    public long Owner { get; set; }

    [JsonPropertyName("source_data")]
    public PaymobSourceDataDto? SourceData { get; set; }

    [JsonPropertyName("merchant_order_id")]
    public string? MerchantOrderId { get; set; }
}

public class PaymobOrderRefDto
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("merchant_order_id")]
    public string? MerchantOrderId { get; set; }
}

public class PaymobSourceDataDto
{
    [JsonPropertyName("pan")]
    public string? Pan { get; set; }

    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("sub_type")]
    public string? SubType { get; set; }
}
