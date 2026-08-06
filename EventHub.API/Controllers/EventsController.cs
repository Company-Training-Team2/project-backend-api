using EventHub.Application.DTOs;
using EventHub.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;
    private readonly IGuestService _guestService;

    public EventsController(
        IEventService eventService,
        IGuestService guestService)
    {
        _eventService = eventService;
        _guestService = guestService;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        return int.Parse(userIdClaim!);
    }


    // ========== EVENT CRUD ==========

    [HttpPost]
    public async Task<ActionResult<EventResponse>> CreateEvent(
        [FromBody] CreateEventRequest request)
    {
        var userId = GetCurrentUserId();

        var evt = await _eventService.CreateEventAsync(userId, request);

        return CreatedAtAction(
            nameof(GetEvent),
            new { id = evt.Id },
            evt);
    }


    [HttpGet]
    public async Task<ActionResult<IEnumerable<EventResponse>>> GetMyEvents()
    {
        var userId = GetCurrentUserId();

        var events = await _eventService.GetUserEventsAsync(userId);

        return Ok(events);
    }


    [HttpGet("{id}")]
    public async Task<ActionResult<EventResponse>> GetEvent(int id)
    {
        var userId = GetCurrentUserId();

        var evt = await _eventService.GetEventByIdAsync(id, userId);

        if (evt == null)
            return NotFound(new { message = "Event not found" });

        return Ok(evt);
    }


    [HttpPut("{id}")]
    public async Task<ActionResult<EventResponse>> UpdateEvent(
        int id,
        [FromBody] UpdateEventRequest request)
    {
        var userId = GetCurrentUserId();

        var evt = await _eventService.UpdateEventAsync(id, userId, request);

        if (evt == null)
            return NotFound(new { message = "Event not found" });

        return Ok(evt);
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var userId = GetCurrentUserId();

        var result = await _eventService.DeleteEventAsync(id, userId);

        if (!result)
            return NotFound(new { message = "Event not found" });

        return NoContent();
    }


    // ========== DASHBOARD ==========

    [HttpGet("{id}/dashboard")]
    public async Task<ActionResult<EventDashboardResponse>> GetDashboard(int id)
    {
        var userId = GetCurrentUserId();

        var dashboard =
            await _eventService.GetEventDashboardAsync(id, userId);

        if (dashboard == null)
            return NotFound(new { message = "Event not found" });

        return Ok(dashboard);
    }


    // ========== VENDORS ==========

    [HttpGet("{id}/vendors")]
    public async Task<ActionResult<IEnumerable<EventVendorResponse>>> GetVendors(int id)
    {
        var userId = GetCurrentUserId();

        var vendors =
            await _eventService.GetEventVendorsAsync(id, userId);

        return Ok(vendors);
    }


    // ========== GUESTS ==========

    [HttpPost("{id}/guests")]
    public async Task<ActionResult<GuestResponse>> AddGuest(
        int id,
        [FromBody] CreateGuestRequest request)
    {
        var userId = GetCurrentUserId();

        var guest = await _guestService.AddGuestAsync(id, userId, request);

        if (guest == null)
            return NotFound(new { message = "Event not found" });

        return CreatedAtAction(
            nameof(GetGuests),
            new { id },
            guest);
    }


    [HttpGet("{id}/guests")]
    public async Task<ActionResult<IEnumerable<GuestResponse>>> GetGuests(int id)
    {
        var userId = GetCurrentUserId();

        var guests =
            await _guestService.GetEventGuestsAsync(id, userId);

        return Ok(guests);
    }


    [HttpPatch("guests/{guestId}/rsvp")]
    public async Task<IActionResult> UpdateRSVP(
        int guestId,
        [FromBody] string status)
    {
        var userId = GetCurrentUserId();

        var result =
            await _guestService.UpdateRSVPStatusAsync(guestId, userId, status);

        if (!result)
            return BadRequest(new
            {
                message = "Invalid RSVP status or guest not found"
            });

        return NoContent();
    }


    [HttpDelete("guests/{guestId}")]
    public async Task<IActionResult> RemoveGuest(int guestId)
    {
        var userId = GetCurrentUserId();

        var result =
            await _guestService.RemoveGuestAsync(guestId, userId);

        if (!result)
            return NotFound(new { message = "Guest not found" });

        return NoContent();
    }
}