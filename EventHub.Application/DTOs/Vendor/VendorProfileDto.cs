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

public class UpdateVendorProfileDto
{
    public string? BusinessName { get; set; }
    public string? BioDescription { get; set; }
    public string? PhoneNumber { get; set; }
    public string? City { get; set; }
    public string? LogoUrl { get; set; }

    // ─── Bank account (Payment module) ────────────────────────────────────────
    public string? BankName { get; set; }
    public string? AccountName { get; set; }
    public string? AccountNumber { get; set; }
}