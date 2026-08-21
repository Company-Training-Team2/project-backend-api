using EventHub.Domain.Common;

namespace EventHub.Domain.Entities;

/// <summary>
/// A free-standing gallery photo on a VendorProfile — the "Image Gallery
/// (up to 10)" collected on Vendor Registration Step 2. Distinct from
/// WorkPostImage (photos scoped to one specific service listing): these
/// showcase the business generally, independent of any one WorkPost, and
/// exist even before the vendor has created their first service.
/// </summary>
public class VendorPortfolioImage : BaseEntity
{
    public int VendorProfileId { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>Upload order — preserves the order the vendor added photos in.</summary>
    public int DisplayOrder { get; set; }

    // ─── Navigation Properties ────────────────────────────────────────────────
    public VendorProfile VendorProfile { get; set; } = null!;
}
