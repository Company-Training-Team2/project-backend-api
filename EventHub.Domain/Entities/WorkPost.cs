using EventHub.Domain.Common;
using EventHub.Domain.Enums;

namespace EventHub.Domain.Entities;

public class WorkPost : SoftDeletableEntity
{
    public int VendorProfileId { get; set; }

    public int CategoryId { get; set; }

    public int? ReviewedByAdminId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string City { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;

    // Navigation Properties

    public VendorProfile VendorProfile { get; set; } = null!;

    public Category Category { get; set; } = null!;

    public User? ReviewedByAdmin { get; set; }

    public ICollection<WorkPostImage> Images { get; set; } = new List<WorkPostImage>();

    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}