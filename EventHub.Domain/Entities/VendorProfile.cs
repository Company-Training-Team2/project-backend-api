using EventHub.Domain.Common;
using EventHub.Domain.Enums;

namespace EventHub.Domain.Entities;

/// <summary>
/// Profile data for Vendor-role users.
/// Vendors require admin KYC approval before they can log in (ApprovalStatus).
/// </summary>
public class VendorProfile : SoftDeletableEntity
{
    public int UserId { get; set; }

    public string BusinessName { get; set; } = string.Empty;

    public string BioDescription { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string? City { get; set; }

    public string? LogoUrl { get; set; }

    /// <summary>Public marketing image (Step 2 "Cover Image"). Served from wwwroot - safe to be public.</summary>
    public string? CoverImageUrl { get; set; }

    // ─── Verification documents (collected at registration, reviewed during
    // admin KYC approval) ──────────────────────────────────────────────────
    // These hold an opaque relative path from IFileStorageService.SavePrivateAsync,
    // NOT a public URL - the files live outside wwwroot and can only be read
    // back via the admin-only GET /api/admin/vendors/{id}/documents/{type}
    // endpoint, never served directly.
    public string? CommercialRegistrationPath { get; set; }

    public string? NationalIdPath { get; set; }

    public string? BusinessLicensePath { get; set; }

    public bool IsVerified { get; set; } = false;

    public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;

    // ─── Bank account (Payment module — required for Payout processing) ──────
    public string? BankName { get; set; }

    public string? AccountName { get; set; }

    /// <summary>Bank account number or IBAN — whichever the vendor provides.</summary>
    public string? AccountNumber { get; set; }

    // ─── Navigation Properties ────────────────────────────────────────────────
    public User User { get; set; } = null!;

    public ICollection<WorkPost> WorkPosts { get; set; } = new List<WorkPost>();

    public ICollection<Payout> Payouts { get; set; } = new List<Payout>();

    /// <summary>Service categories selected at registration (up to 3). See VendorProfileCategory.</summary>
    public ICollection<VendorProfileCategory> VendorCategories { get; set; } = new List<VendorProfileCategory>();

    /// <summary>General storefront gallery photos (up to 10), collected at
    /// registration Step 2. See VendorPortfolioImage.</summary>
    public ICollection<VendorPortfolioImage> PortfolioImages { get; set; } = new List<VendorPortfolioImage>();
}