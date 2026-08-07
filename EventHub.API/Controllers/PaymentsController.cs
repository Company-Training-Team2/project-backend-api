using EventHub.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.API.Controllers;

/// <summary>
/// Payment module: customer-facing checkout via Paymob.
/// Requires authentication — payments are scoped to the logged-in customer.
/// </summary>
[ApiController]
[Route("api/payments")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    /// <summary>POST /api/payments/checkout/{bookingId} — starts a Paymob checkout session for a Confirmed booking.</summary>
    [HttpPost("checkout/{bookingId}")]
    public async Task<IActionResult> Checkout(int bookingId)
    {
        var result = await _paymentService.InitiateCheckoutAsync(bookingId);
        return Ok(result);
    }

    /// <summary>GET /api/payments/my — payment history for the current customer.</summary>
    [HttpGet("my")]
    public async Task<IActionResult> GetMyPayments()
    {
        var result = await _paymentService.GetMyPaymentsAsync();
        return Ok(result);
    }

    /// <summary>GET /api/payments/{bookingId} — the payment for a specific booking.</summary>
    [HttpGet("{bookingId}")]
    public async Task<IActionResult> GetByBookingId(int bookingId)
    {
        var result = await _paymentService.GetByBookingIdAsync(bookingId);
        return Ok(result);
    }
}
