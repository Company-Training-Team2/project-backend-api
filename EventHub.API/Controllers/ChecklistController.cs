using EventHub.Application.DTOs.Checklist;
using EventHub.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventHub.API.Controllers;

/// <summary>
/// Module 5 – Checklist task management.
///
/// Routes:
///   GET    /api/events/{id}/checklist           → grouped Pending / Completed response
///   POST   /api/events/{id}/checklist           → create new task
///   PUT    /api/checklist/{id}                  → full update
///   DELETE /api/checklist/{id}                  → remove task
///   PATCH  /api/checklist/{id}/toggle           → flip IsCompleted
/// </summary>
[ApiController]
[Authorize]
public class ChecklistController : ControllerBase
{
    private readonly IChecklistService _checklistService;

    public ChecklistController(IChecklistService checklistService)
    {
        _checklistService = checklistService;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim!);
    }

    // ── GET /api/events/{id}/checklist ────────────────────────────────────────

    [HttpGet("api/events/{id}/checklist")]
    public async Task<ActionResult<EventChecklistResponse>> GetChecklist(int id)
    {
        var userId = GetCurrentUserId();

        var result = await _checklistService.GetChecklistAsync(id, userId);

        if (result == null)
            return NotFound(new { message = "Event not found or access denied." });

        return Ok(result);
    }

    // ── POST /api/events/{id}/checklist ───────────────────────────────────────

    [HttpPost("api/events/{id}/checklist")]
    public async Task<ActionResult<ChecklistItemDto>> CreateChecklistItem(
        int id,
        [FromBody] CreateChecklistItemRequest request)
    {
        var userId = GetCurrentUserId();

        var item = await _checklistService.CreateChecklistItemAsync(id, userId, request);

        if (item == null)
            return NotFound(new { message = "Event not found or access denied." });

        return CreatedAtAction(
            nameof(GetChecklist),
            new { id },
            item);
    }

    // ── PUT /api/checklist/{id} ───────────────────────────────────────────────

    [HttpPut("api/checklist/{id}")]
    public async Task<ActionResult<ChecklistItemDto>> UpdateChecklistItem(
        int id,
        [FromBody] UpdateChecklistItemRequest request)
    {
        var userId = GetCurrentUserId();

        var item = await _checklistService.UpdateChecklistItemAsync(id, userId, request);

        if (item == null)
            return NotFound(new { message = "Checklist item not found or access denied." });

        return Ok(item);
    }

    // ── DELETE /api/checklist/{id} ────────────────────────────────────────────

    [HttpDelete("api/checklist/{id}")]
    public async Task<IActionResult> DeleteChecklistItem(int id)
    {
        var userId = GetCurrentUserId();

        var deleted = await _checklistService.DeleteChecklistItemAsync(id, userId);

        if (!deleted)
            return NotFound(new { message = "Checklist item not found or access denied." });

        return NoContent();
    }

    // ── PATCH /api/checklist/{id}/toggle ─────────────────────────────────────

    [HttpPatch("api/checklist/{id}/toggle")]
    public async Task<ActionResult<ChecklistItemDto>> ToggleChecklistItem(int id)
    {
        var userId = GetCurrentUserId();

        var item = await _checklistService.ToggleChecklistItemAsync(id, userId);

        if (item == null)
            return NotFound(new { message = "Checklist item not found or access denied." });

        return Ok(item);
    }
}