using EventHub.Application.DTOs.Booking;
using EventHub.Application.Interfaces;
using EventHub.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.API.Controllers;

// Had no [Authorize] at all — every action here (including Create, Accept,
// Reject, Cancel, and reading any customer's/vendor's full booking list by
// id) was reachable by anyone, logged in or not. [Authorize] requires a
// valid token for all of them; BookingService then checks the caller is
// actually the customer/vendor party to that specific booking (or an
// Admin) — see its GetCurrent*ProfileAsync/EnsureCurrentUserOwnsWorkPostAsync
// helpers, same ownership pattern FavoriteService/VendorController use.
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookingController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateBookingDto dto)
    {
        try
        {
            var result = await _bookingService.CreateAsync(dto);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpPut("{id}/accept")]
    public async Task<IActionResult> Accept(int id)
    {
        try
        {
            var result = await _bookingService.AcceptAsync(id);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpPut("{id}/reject")]
    public async Task<IActionResult> Reject(int id)
    {
        try
        {
            var result = await _bookingService.RejectAsync(id);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        try
        {
            var result = await _bookingService.CancelAsync(id);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var result = await _bookingService.GetByIdAsync(id);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    /// <summary>GET /api/bookings/customer/{customerId}?status=Pending — status filter per audit Module 8 API contract.</summary>
    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetCustomerBookings(int customerId, [FromQuery] BookingStatus? status)
    {
        try
        {
            var result = await _bookingService.GetCustomerBookingsAsync(customerId, status);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    /// <summary>GET /api/bookings/vendor/{vendorId}?status=Pending</summary>
    [HttpGet("vendor/{vendorId}")]
    public async Task<IActionResult> GetVendorBookings(int vendorId, [FromQuery] BookingStatus? status)
    {
        try
        {
            var result = await _bookingService.GetVendorBookingsAsync(vendorId, status);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }
}