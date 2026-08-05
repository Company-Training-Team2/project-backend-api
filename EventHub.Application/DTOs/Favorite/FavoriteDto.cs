using EventHub.Application.DTOs.WorkPost;

namespace EventHub.Application.DTOs.Favorite;

/// <summary>
/// Audit Module 9: a saved favorite enriched with WorkPost card-level
/// projection data, as consumed by the consolidated Favorites dashboard.
/// </summary>
public class FavoriteDto
{
    public int FavoriteId { get; set; }

    public WorkPostSummaryDto WorkPost { get; set; } = null!;
}
