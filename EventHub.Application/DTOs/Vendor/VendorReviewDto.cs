namespace EventHub.Application.DTOs.Vendor;

/// <summary>GET /api/vendor/reviews — reviews across every one of the vendor's WorkPosts.</summary>
public class VendorReviewDto
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string WorkPostTitle { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}
