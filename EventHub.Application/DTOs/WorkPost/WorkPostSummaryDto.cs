namespace EventHub.Application.DTOs.WorkPost;

/// <summary>
/// Card-level projection of a WorkPost — used by GET /api/workposts/search
/// and by the "recommended" section of GET /api/home/dashboard.
/// </summary>
public class WorkPostSummaryDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string CategoryName { get; set; } = string.Empty;

    public string VendorBusinessName { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string? PrimaryImageUrl { get; set; }

    public double AverageRating { get; set; }

    public int ReviewCount { get; set; }
}
