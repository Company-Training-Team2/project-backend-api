using EventHub.Application.DTOs.Payment;
using EventHub.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.API.Controllers;

/// <summary>
/// Payment module: Paymob's server-to-server Transaction Processed Callback.
/// Not user-authenticated — trust is established purely via HMAC verification
/// inside IPaymentService.HandleGatewayCallbackAsync, so this endpoint must
/// stay anonymous (Paymob's servers don't send a JWT) but never trusts the
/// payload without a valid signature.
/// </summary>
[ApiController]
[Route("api/payments/webhook")]
[AllowAnonymous]
public class PaymentWebhookController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentWebhookController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    /// <summary>POST /api/payments/webhook?hmac=... — body is Paymob's { "type": "TRANSACTION", "obj": {...} } envelope.</summary>
    [HttpPost]
    public async Task<IActionResult> HandleCallback(
        [FromBody] PaymobWebhookEnvelope envelope,
        [FromQuery] string hmac)
    {
        await _paymentService.HandleGatewayCallbackAsync(envelope.Obj, hmac);

        // Paymob only checks for a 2xx response; body content is ignored.
        return Ok();
    }
}
