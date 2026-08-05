using EventHub.Application.DTOs.WorkPost;
using EventHub.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.API.Controllers;

/// <summary>
/// Audit Module 2 (Home &amp; Discovery): Browse/Search -> Vendor Details.
/// Both endpoints are public — browsing doesn't require login.
/// </summary>
[ApiController]
[Route("api/workposts")]
[AllowAnonymous]
public class WorkPostController : ControllerBase
{
    private readonly IWorkPostService _workPostService;

    public WorkPostController(IWorkPostService workPostService)
    {
        _workPostService = workPostService;
    }

    /// <summary>
    /// GET /api/workposts/search?category=&amp;city=&amp;price_range=100-500&amp;rating=4
    /// </summary>
    [HttpGet("search")]
    public async Task<IActionResult> Search(
        [FromQuery] string? category,
        [FromQuery] string? city,
        [FromQuery(Name = "price_range")] string? priceRange,
        [FromQuery] double? rating,
        [FromQuery] string? q,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12)
    {
        decimal? minPrice = null;
        decimal? maxPrice = null;

        if (!string.IsNullOrWhiteSpace(priceRange))
        {
            var parts = priceRange.Split(
                '-',
                StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length > 0 && decimal.TryParse(parts[0], out var min))
                minPrice = min;

            if (parts.Length > 1 && decimal.TryParse(parts[1], out var max))
                maxPrice = max;
        }

        var query = new WorkPostSearchQuery
        {
            Category = category,
            City = city,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            MinRating = rating,
            Keyword = q,
            Page = page,
            PageSize = pageSize
        };

        var result = await _workPostService.SearchAsync(query);

        return Ok(result);
    }

    /// <summary>GET /api/workposts/{id}</summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _workPostService.GetDetailAsync(id);

        return Ok(result);
    }
}
