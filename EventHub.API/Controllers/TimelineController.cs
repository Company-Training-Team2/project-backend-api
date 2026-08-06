using EventHub.Application.DTOs.Timeline;
using EventHub.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventHub.API.Controllers;

/// <summary>
/// Module 6 – Dynamic milestone timeline.
///
/// Routes:
///   GET /api/events/{id}/timeline  → computed milestone status payload
/// </summary>
[ApiController]
[Authorize]
public class TimelineController : ControllerBase
{
    private readonly ITimelineService _timelineService;

    public TimelineController(ITimelineService timelineService)
    {
        _timelineService = timelineService;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim!);
    }

    // ── GET /api/events/{id}/timeline ─────────────────────────────────────────

    [HttpGet("api/events/{id}/timeline")]
    public async Task<ActionResult<TimelineResponse>> GetTimeline(int id)
    {
        var userId = GetCurrentUserId();

        var result = await _timelineService.GetTimelineAsync(id, userId);

        if (result == null)
            return NotFound(new { message = "Event not found or access denied." });

        return Ok(result);
    }
}