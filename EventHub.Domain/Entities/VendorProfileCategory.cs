using EventHub.Domain.Common;

namespace EventHub.Domain.Entities;

/// <summary>
/// Join row: which service categories a vendor selected during registration
/// (up to 3 — see CategoryChipSelect on the frontend / RegisterRequest.CategoryIds).
/// Distinct from WorkPost.CategoryId, which is the single category a specific
/// service listing belongs to.
/// </summary>
public class VendorProfileCategory : BaseEntity
{
    public int VendorProfileId { get; set; }

    public int CategoryId { get; set; }

    public VendorProfile VendorProfile { get; set; } = null!;

    public Category Category { get; set; } = null!;
}
