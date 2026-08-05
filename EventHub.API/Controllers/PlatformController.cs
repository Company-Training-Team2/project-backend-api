using EventHub.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.API.Controllers;

/// <summary>
/// Audit Module 2: Public Stats — unauthenticated marketing metrics.
/// </summary>
[ApiController]
[Route("api/platform")]
[AllowAnonymous]
public class PlatformController : ControllerBase
{
    private readonly IPlatformService _platformService;

    public PlatformController(IPlatformService platformService)
    {
        _platformService = platformService;
    }

    /// <summary>GET /api/platform/stats</summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var result = await _platformService.GetStatsAsync();

        return Ok(result);
    }
}
