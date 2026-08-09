using EventHub.Application.DTOs.Payment;
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
    private readonly IPaymentMethodService _paymentMethodService;

    public PaymentsController(IPaymentService paymentService, IPaymentMethodService paymentMethodService)
    {
        _paymentService = paymentService;
        _paymentMethodService = paymentMethodService;
    }

    /// <summary>POST /api/payments/checkout/{bookingId} — starts a Paymob checkout session for an Accepted booking.</summary>
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
    [HttpGet("{bookingId:int}")]
    public async Task<IActionResult> GetByBookingId(int bookingId)
    {
        var result = await _paymentService.GetByBookingIdAsync(bookingId);
        return Ok(result);
    }

    // ───────────────── Saved payment methods (audit Module 8) ─────────────────

    /// <summary>GET /api/payments/methods — the current customer's saved payment instruments.</summary>
    [HttpGet("methods")]
    public async Task<IActionResult> GetMyPaymentMethods()
    {
        var result = await _paymentMethodService.GetMyPaymentMethodsAsync();
        return Ok(result);
    }

    /// <summary>POST /api/payments/methods — save a new payment instrument (masked/tokenized only, never a raw card number).</summary>
    [HttpPost("methods")]
    public async Task<IActionResult> AddPaymentMethod([FromBody] CreateSavedPaymentMethodRequest request)
    {
        var result = await _paymentMethodService.AddPaymentMethodAsync(request);
        return Ok(result);
    }

    /// <summary>DELETE /api/payments/methods/{id} — remove a saved payment instrument.</summary>
    [HttpDelete("methods/{id}")]
    public async Task<IActionResult> DeletePaymentMethod(int id)
    {
        var deleted = await _paymentMethodService.DeletePaymentMethodAsync(id);

        if (!deleted)
            return NotFound(new { message = "Payment method not found or access denied." });

        return NoContent();
    }
}
