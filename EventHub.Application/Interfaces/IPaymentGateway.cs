using EventHub.Application.DTOs.Payment;

namespace EventHub.Application.Interfaces;

/// <summary>
/// Payment module: transport-agnostic abstraction over the payment gateway
/// (Auth token → Order registration → Payment key → Iframe checkout). Paymob
/// is the only implementation today (PaymobPaymentGateway in Infrastructure),
/// but IPaymentService only depends on this interface so the gateway can be
/// swapped or mocked in tests.
/// </summary>
public interface IPaymentGateway
{
    /// <summary>Runs the full Paymob flow and returns an iframe/unified checkout URL.</summary>
    Task<PaymentGatewayResult> CreatePaymentKeyAsync(PaymentGatewayRequest request);

    /// <summary>Verifies the HMAC signature Paymob sends with a transaction callback.</summary>
    bool VerifyWebhookSignature(string receivedHmac, PaymobTransactionCallbackDto callback);
}
