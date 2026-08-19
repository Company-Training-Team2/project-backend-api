namespace EventHub.Application.DTOs.Admin;

/// <summary>
/// A vendor's service listing (WorkPost) as seen by Admin — used in the
/// service approval queue (/admin/workposts/pending) and directory
/// (/admin/workposts). Mirrors AdminVendorDto's role for vendor accounts,
/// but for the individual listings a vendor publishes underneath their
/// (already-approved) account.
/// </summary>
public class AdminWorkPostDto
{
    public int Id { get; set; }

    public int VendorProfileId { get; set; }

    public string VendorBusinessName { get; set; } = string.Empty;

    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public string City { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public int? MinGuests { get; set; }

    public int? MaxGuests { get; set; }

    public string ApprovalStatus { get; set; } = string.Empty;

    public string? PrimaryImageUrl { get; set; }

    /// <summary>All uploaded photos, primary first — the queue list uses
    /// PrimaryImageUrl alone, but the review detail view needs the full set
    /// to actually judge a new listing.</summary>
    public List<string> ImageUrls { get; set; } = new();

    public DateTime CreatedAt { get; set; }
}
