using System.Security.Claims;
using EventHub.Application.DTOs.Vendor;
using EventHub.Application.DTOs.WorkPost;
using EventHub.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.API.Controllers;

/// <summary>
/// Module 13 — Vendor Portal.
/// All endpoints require the "Vendor" role.
/// </summary>
[ApiController]
[Route("api/vendor")]
[Authorize(Roles = "Vendor")]
public class VendorController : ControllerBase
{
    private readonly IVendorService _vendorService;
    private readonly IPaymentService _paymentService;
    private readonly IPayoutService _payoutService;

    public VendorController(
        IVendorService vendorService,
        IPaymentService paymentService,
        IPayoutService payoutService)
    {
        _vendorService = vendorService;
        _paymentService = paymentService;
        _payoutService = payoutService;
    }

    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ── Dashboard ────────────────────────────────────────────────────────────

    /// <summary>GET /api/vendor/dashboard</summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var result = await _vendorService.GetDashboardAsync(CurrentUserId);
        return Ok(result);
    }

    // ── WorkPost (Services) CRUD ─────────────────────────────────────────────

    /// <summary>GET /api/vendor/services</summary>
    [HttpGet("services")]
    public async Task<IActionResult> GetWorkPosts()
    {
        var result = await _vendorService.GetMyWorkPostsAsync(CurrentUserId);
        return Ok(result);
    }

    /// <summary>GET /api/vendor/services/{id}</summary>
    [HttpGet("services/{id}")]
    public async Task<IActionResult> GetWorkPost(int id)
    {
        var result = await _vendorService.GetWorkPostByIdAsync(CurrentUserId, id);
        return Ok(result);
    }

    /// <summary>POST /api/vendor/services</summary>
    [HttpPost("services")]
    public async Task<IActionResult> CreateWorkPost([FromBody] CreateWorkPostDto dto)
    {
        var result = await _vendorService.CreateWorkPostAsync(CurrentUserId, dto);
        return CreatedAtAction(nameof(GetWorkPost), new { id = result.Id }, result);
    }

    /// <summary>PUT /api/vendor/services/{id}</summary>
    [HttpPut("services/{id}")]
    public async Task<IActionResult> UpdateWorkPost(int id, [FromBody] UpdateWorkPostDto dto)
    {
        var result = await _vendorService.UpdateWorkPostAsync(CurrentUserId, id, dto);
        return Ok(result);
    }

    /// <summary>DELETE /api/vendor/services/{id}</summary>
    [HttpDelete("services/{id}")]
    public async Task<IActionResult> DeleteWorkPost(int id)
    {
        await _vendorService.DeleteWorkPostAsync(CurrentUserId, id);
        return NoContent();
    }

    // ── Availability ──────────────────────────────────────────────────────────

    /// <summary>GET /api/vendor/availability</summary>
    [HttpGet("availability")]
    public async Task<IActionResult> GetAvailability()
    {
        var result = await _vendorService.GetAvailabilityAsync(CurrentUserId);
        return Ok(result);
    }

    /// <summary>PUT /api/vendor/availability</summary>
    [HttpPut("availability")]
    public async Task<IActionResult> UpdateAvailability([FromBody] UpdateAvailabilityDto dto)
    {
        await _vendorService.UpdateAvailabilityAsync(CurrentUserId, dto);
        return NoContent();
    }

    // ── Bookings ──────────────────────────────────────────────────────────────

    /// <summary>GET /api/vendor/bookings?status=pending</summary>
    [HttpGet("bookings")]
    public async Task<IActionResult> GetBookings([FromQuery] string? status)
    {
        var result = await _vendorService.GetBookingsAsync(CurrentUserId, status);
        return Ok(result);
    }

    /// <summary>PUT /api/vendor/bookings/{id}/approve</summary>
    [HttpPut("bookings/{id}/approve")]
    public async Task<IActionResult> ApproveBooking(int id)
    {
        var result = await _vendorService.ApproveBookingAsync(CurrentUserId, id);
        return Ok(result);
    }

    /// <summary>PUT /api/vendor/bookings/{id}/decline</summary>
    [HttpPut("bookings/{id}/decline")]
    public async Task<IActionResult> DeclineBooking(int id)
    {
        var result = await _vendorService.DeclineBookingAsync(CurrentUserId, id);
        return Ok(result);
    }

    /// <summary>PUT /api/vendor/bookings/{id}/complete — marks a Paid booking as delivered and triggers the vendor's Payout.</summary>
    [HttpPut("bookings/{id}/complete")]
    public async Task<IActionResult> CompleteBooking(int id)
    {
        var result = await _vendorService.CompleteBookingAsync(CurrentUserId, id);
        return Ok(result);
    }

    // ── Analytics ────────────────────────────────────────────────────────────

    /// <summary>GET /api/vendor/analytics</summary>
    [HttpGet("analytics")]
    public async Task<IActionResult> GetAnalytics()
    {
        var result = await _vendorService.GetAnalyticsAsync(CurrentUserId);
        return Ok(result);
    }

    // ── Reviews ──────────────────────────────────────────────────────────────

    /// <summary>GET /api/vendor/reviews</summary>
    [HttpGet("reviews")]
    public async Task<IActionResult> GetReviews()
    {
        var result = await _vendorService.GetReviewsAsync(CurrentUserId);
        return Ok(result);
    }

    // ── Profile ───────────────────────────────────────────────────────────────

    /// <summary>GET /api/vendor/profile</summary>
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var result = await _vendorService.GetProfileAsync(CurrentUserId);
        return Ok(result);
    }

    /// <summary>PUT /api/vendor/profile</summary>
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateVendorProfileDto dto)
    {
        var result = await _vendorService.UpdateProfileAsync(CurrentUserId, dto);
        return Ok(result);
    }

    /// <summary>POST /api/vendor/profile/logo — replaces the storefront logo
    /// with an uploaded image (multipart/form-data, field name "file"). Was a
    /// free-text LogoUrl string on UpdateProfile with no validation at all;
    /// see UpdateVendorProfileDto's doc comment.</summary>
    [HttpPost("profile/logo")]
    public async Task<IActionResult> UploadLogo(IFormFile file)
    {
        try
        {
            var result = await _vendorService.UploadLogoAsync(CurrentUserId, file);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ── WorkPost Images ───────────────────────────────────────────────────────

    /// <summary>
    /// POST /api/vendor/services/{id}/images
    /// Upload one or more images for a WorkPost.
    /// Depends on Blob Infrastructure: stores the raw URL returned by the
    /// upstream blob handler (or falls back to a placeholder when no blob
    /// service is wired yet so the endpoint stays testable end-to-end).
    /// </summary>
    [HttpPost("services/{id:int}/images")]
    public async Task<IActionResult> UploadWorkPostImages(
        int id,
        [FromForm] UploadWorkPostImagesRequest request)
    {
        try
        {
            var result = await _vendorService.UploadWorkPostImagesAsync(
                CurrentUserId, id, request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    // ── Payments (Payment module) ────────────────────────────────────────────

    /// <summary>GET /api/vendor/earnings</summary>
    [HttpGet("earnings")]
    public async Task<IActionResult> GetEarnings()
    {
        var result = await _paymentService.GetVendorEarningsAsync(CurrentUserId);
        return Ok(result);
    }

    /// <summary>GET /api/vendor/payouts</summary>
    [HttpGet("payouts")]
    public async Task<IActionResult> GetPayouts()
    {
        var result = await _payoutService.GetMyPayoutsAsync(CurrentUserId);
        return Ok(result);
    }
}
