namespace EventHub.Application.DTOs.WorkPost;

/// <summary>
/// Full payload for GET /api/workposts/{id} — vendor details screen.
/// </summary>
public class WorkPostDetailDto
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string City { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string CategoryName { get; set; } = string.Empty;

    public int VendorProfileId { get; set; }

    public string VendorBusinessName { get; set; } = string.Empty;

    public string? VendorLogoUrl { get; set; }

    public bool VendorIsVerified { get; set; }

    public double AverageRating { get; set; }

    public int ReviewCount { get; set; }

    public List<WorkPostImageDto> Images { get; set; } = new();

    public List<ServicePackageDto> ServicePackages { get; set; } = new();

    public List<ReviewSummaryDto> Reviews { get; set; } = new();

    public List<AvailableSlotDto> AvailableTimeSlots { get; set; } = new();
}
