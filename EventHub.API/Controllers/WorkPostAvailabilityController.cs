using EventHub.Application.DTOs.WorkPostAvailability;
using EventHub.Application.Interfaces;
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

    // POST: api/WorkPostAvailability
    [HttpPost]
    public async Task<IActionResult> Create(CreateWorkPostAvailabilityDto dto)
    {
        var result = await _availabilityService.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetByWorkPost),
            new { workPostId = result.WorkPostId },
            result);
    }

    // PUT: api/WorkPostAvailability/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateWorkPostAvailabilityDto dto)
    {
        var result = await _availabilityService.UpdateAsync(id, dto);

        return Ok(result);
    }

    // DELETE: api/WorkPostAvailability/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _availabilityService.DeleteAsync(id);

        return NoContent();
    }

    // GET: api/WorkPostAvailability/workpost/3
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