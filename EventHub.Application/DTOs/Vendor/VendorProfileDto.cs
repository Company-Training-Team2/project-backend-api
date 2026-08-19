using System.ComponentModel.DataAnnotations;

namespace EventHub.Application.DTOs.Vendor;

/// <summary>
/// GET /PUT /api/vendor/profile — public storefront profile.
/// </summary>
public class VendorProfileDto
{
    public int Id { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public string BioDescription { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? City { get; set; }
    public string? LogoUrl { get; set; }
    public bool IsVerified { get; set; }
    public string ApprovalStatus { get; set; } = string.Empty;

    // ─── Bank account (Payment module — required for Payout processing) ──────
    public string? BankName { get; set; }
    public string? AccountName { get; set; }
    public string? AccountNumber { get; set; }
}

/// <summary>
/// Max lengths mirror VendorProfileConfiguration.cs's column caps, so an
/// oversized value is rejected here (400, via [ApiController]'s automatic
/// ModelState validation) instead of failing later with a raw SQL/DB error.
/// Previously had no validation attributes at all — every field, including
/// LogoUrl, accepted absolutely anything.
///
/// LogoUrl is deliberately NOT here anymore. It used to be a free-text URL
/// field with zero validation (any string, including plain non-URL text,
/// was accepted and rendered as an <img src>) — replaced by a real upload:
/// see UploadLogoAsync / POST /api/vendor/profile/logo.
/// </summary>
public class UpdateVendorProfileDto
{
    [MaxLength(200, ErrorMessage = "Business name must be 200 characters or fewer.")]
    public string? BusinessName { get; set; }

    [MaxLength(2000, ErrorMessage = "Bio must be 2000 characters or fewer.")]
    public string? BioDescription { get; set; }

    [Phone(ErrorMessage = "Enter a valid phone number.")]
    [MaxLength(20, ErrorMessage = "Phone number must be 20 characters or fewer.")]
    public string? PhoneNumber { get; set; }

    [MaxLength(100, ErrorMessage = "City must be 100 characters or fewer.")]
    public string? City { get; set; }

    // ─── Bank account (Payment module) ────────────────────────────────────────
    [MaxLength(200, ErrorMessage = "Bank name must be 200 characters or fewer.")]
    public string? BankName { get; set; }

    [MaxLength(200, ErrorMessage = "Account name must be 200 characters or fewer.")]
    public string? AccountName { get; set; }

    [MaxLength(50, ErrorMessage = "Account number must be 50 characters or fewer.")]
    public string? AccountNumber { get; set; }
}