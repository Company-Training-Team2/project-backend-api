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
}