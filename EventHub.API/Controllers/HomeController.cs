using EventHub.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.API.Controllers;

/// <summary>
/// Audit Module 2 (Home &amp; Discovery): Home (Summary Dashboard).
/// Requires authentication — the dashboard is scoped to the logged-in customer.
/// </summary>
[ApiController]
[Route("api/home")]
[Authorize]
public class HomeController : ControllerBase
{
    private readonly IHomeService _homeService;

    public HomeController(IHomeService homeService)
    {
        _homeService = homeService;
    }

    /// <summary>GET /api/home/dashboard</summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var result = await _homeService.GetDashboardAsync();

        return Ok(result);
    }
}
