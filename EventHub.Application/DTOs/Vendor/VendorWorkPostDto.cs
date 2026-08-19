using EventHub.Application.DTOs.WorkPost;

namespace EventHub.Application.DTOs.Vendor;

/// <summary>
/// Enriched WorkPost card shown inside the vendor's own portal
/// (includes approval status, image list, package count, etc.)
/// </summary>
public class VendorWorkPostDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string City { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public int? MinGuests { get; set; }
    public int? MaxGuests { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string ApprovalStatus { get; set; } = string.Empty;
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public int TotalBookings { get; set; }
    public List<WorkPostImageDto> Images { get; set; } = new();
    public List<ServicePackageDto> ServicePackages { get; set; } = new();
}