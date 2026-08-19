using EventHub.Application.DTOs.WorkPostAvailability;
using EventHub.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WorkPostAvailabilityController : ControllerBase
{
    private readonly IWorkPostAvailabilityService _availabilityService;

    public WorkPostAvailabilityController(IWorkPostAvailabilityService availabilityService)
    {
        _availabilityService = availabilityService;
    }

    // Create/Update/Delete had no [Authorize] at all — anyone could create,
    // edit, or delete availability slots for any vendor's WorkPost with no
    // login. [Authorize(Roles="Vendor")] plus WorkPostAvailabilityService's
    // EnsureCurrentUserOwnsWorkPostAsync check together require both a vendor
    // login and that the specific WorkPost being touched is actually theirs.

    // POST: api/WorkPostAvailability
    [HttpPost]
    [Authorize(Roles = "Vendor")]
    public async Task<IActionResult> Create(CreateWorkPostAvailabilityDto dto)
    {
        try
        {
            var result = await _availabilityService.CreateAsync(dto);

            return CreatedAtAction(
                nameof(GetByWorkPost),
                new { workPostId = result.WorkPostId },
                result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    // PUT: api/WorkPostAvailability/5
    [HttpPut("{id}")]
    [Authorize(Roles = "Vendor")]
    public async Task<IActionResult> Update(int id, UpdateWorkPostAvailabilityDto dto)
    {
        try
        {
            var result = await _availabilityService.UpdateAsync(id, dto);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    // DELETE: api/WorkPostAvailability/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "Vendor")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _availabilityService.DeleteAsync(id);
            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    // GET: api/WorkPostAvailability/workpost/3
    // Left [AllowAnonymous]-equivalent (no attribute) — read-only calendar
    // availability is needed while browsing a vendor's page before signing
    // in, same as WorkPostController's own public search/detail endpoints.
    [HttpGet("workpost/{workPostId}")]
    public async Task<IActionResult> GetByWorkPost(int workPostId)
    {
        var result = await _availabilityService.GetByWorkPostIdAsync(workPostId);

        return Ok(result);
    }

    // GET: api/WorkPostAvailability/check?workPostId=1&date=2026-08-10
    [HttpGet("check")]
    public async Task<IActionResult> CheckAvailability(
        int workPostId,
        DateOnly date)
    {
        var available = await _availabilityService.IsAvailableAsync(workPostId, date);

        return Ok(new
        {
            WorkPostId = workPostId,
            Date = date,
            IsAvailable = available
        });
    }
}
