using EventHub.Application.DTOs.Booking;
using EventHub.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
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
        var result = await _bookingService.CreateAsync(dto);

        return Ok(result);
    }
    [HttpPut("{id}/accept")]
    public async Task<IActionResult> Accept(int id)
    {
        var result = await _bookingService.AcceptAsync(id);

        return Ok(result);
    }

    [HttpPut("{id}/reject")]
    public async Task<IActionResult> Reject(int id)
    {
        var result = await _bookingService.RejectAsync(id);

        return Ok(result);
    }

    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var result = await _bookingService.CancelAsync(id);

        return Ok(result);
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _bookingService.GetByIdAsync(id);

        return Ok(result);
    }
    [HttpGet("customer/{customerId}")]
    public async Task<IActionResult> GetCustomerBookings(int customerId)
    {
        var result = await _bookingService.GetCustomerBookingsAsync(customerId);

        return Ok(result);
    }
    [HttpGet("vendor/{vendorId}")]
    public async Task<IActionResult> GetVendorBookings(int vendorId)
    {
        var result = await _bookingService.GetVendorBookingsAsync(vendorId);

        return Ok(result);
    }
}