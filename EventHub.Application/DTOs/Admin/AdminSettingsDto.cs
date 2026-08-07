namespace EventHub.Application.DTOs.Admin;

/// <summary>
/// GET /PUT /api/admin/settings
/// Platform-wide configuration: commissions, tax, taxonomy branding.
/// </summary>
public class AdminSettingsDto
{
    // ── Commission ─────────────────────────────────────────────────────────────
    /// <summary>Platform commission percentage taken from each completed booking (0–100).</summary>
    public decimal CommissionPercentage { get; set; }

    // ── Tax ────────────────────────────────────────────────────────────────────
    /// <summary>VAT / tax percentage applied to customer invoices (0–100).</summary>
    public decimal TaxPercentage { get; set; }

    // ── Taxonomy ───────────────────────────────────────────────────────────────
    /// <summary>Maximum number of images allowed per WorkPost.</summary>
    public int MaxImagesPerWorkPost { get; set; }

    /// <summary>Maximum number of ServicePackages allowed per WorkPost.</summary>
    public int MaxPackagesPerWorkPost { get; set; }

    // ── Branding ───────────────────────────────────────────────────────────────
    public string PlatformName { get; set; } = string.Empty;
    public string? PlatformLogoUrl { get; set; }
    public string? SupportEmail { get; set; }

    public DateTime UpdatedAt { get; set; }
}

/// <summary>Partial-update body for PUT /api/admin/settings.</summary>
public class UpdateAdminSettingsDto
{
    public decimal? CommissionPercentage { get; set; }
    public decimal? TaxPercentage { get; set; }
    public int? MaxImagesPerWorkPost { get; set; }
    public int? MaxPackagesPerWorkPost { get; set; }
    public string? PlatformName { get; set; }
    public string? PlatformLogoUrl { get; set; }
    public string? SupportEmail { get; set; }
}
