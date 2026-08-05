namespace EventHub.Application.DTOs.WorkPost;

/// <summary>
/// Service-layer search input. The controller is responsible for parsing the
/// raw "price_range" query string (e.g. "100-500") into MinPrice/MaxPrice
/// before building this.
/// </summary>
public class WorkPostSearchQuery
{
    public string? Category { get; set; }

    public string? City { get; set; }

    public decimal? MinPrice { get; set; }

    public decimal? MaxPrice { get; set; }

    /// <summary>Minimum average rating (e.g. 4 → 4 stars and up).</summary>
    public double? MinRating { get; set; }

    /// <summary>Free-text search over Title / Description.</summary>
    public string? Keyword { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 12;
}
