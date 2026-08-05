using EventHub.Application.DTOs.Favorite;

namespace EventHub.Application.Interfaces;

/// <summary>Audit Module 9 (Favorites).</summary>
public interface IFavoriteService
{
    /// <summary>
    /// Adds the WorkPost to the current customer's favorites if it isn't already
    /// saved, otherwise removes it. Backs the global Heart-icon toggle.
    /// </summary>
    Task<ToggleFavoriteResultDto> ToggleAsync(int workPostId);

    /// <summary>Saved favorites for the current customer, enriched with WorkPost projection data.</summary>
    Task<IEnumerable<FavoriteDto>> GetMyFavoritesAsync();
}
