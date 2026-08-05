using EventHub.Application.DTOs.Favorite;
using EventHub.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventHub.API.Controllers;

/// <summary>
/// Audit Module 9 (Favorites): global toggle interactions (Heart icon) across
/// Vendor Cards / Search views, feeding the consolidated Favorites dashboard.
/// Requires authentication — favorites are scoped to the logged-in customer.
/// </summary>
[ApiController]
[Route("api/favorites")]
[Authorize]
public class FavoritesController : ControllerBase
{
    private readonly IFavoriteService _favoriteService;

    public FavoritesController(IFavoriteService favoriteService)
    {
        _favoriteService = favoriteService;
    }

    /// <summary>POST /api/favorites/toggle — add/remove a vendor (WorkPost) from the user's favorites list.</summary>
    [HttpPost("toggle")]
    public async Task<IActionResult> Toggle(ToggleFavoriteRequest request)
    {
        var result = await _favoriteService.ToggleAsync(request.WorkPostId);

        return Ok(result);
    }

    /// <summary>GET /api/favorites — saved favorites enriched with WorkPost projection data.</summary>
    [HttpGet]
    public async Task<IActionResult> GetMyFavorites()
    {
        var result = await _favoriteService.GetMyFavoritesAsync();

        return Ok(result);
    }
}
