using EventHub.Domain.Common;

namespace EventHub.Domain.Entities;

/// <summary>
/// Singleton platform configuration row (always Id = 1).
/// GET /PUT /api/admin/settings.
/// Covers: commissions, tax, taxonomy limits, and branding.
/// </summary>
public class AdminSettings : BaseEntity
{
    // ── Commission ─────────────────────────────────────────────────────────────
    public decimal CommissionPercentage { get; set; } = 10m;

    // ── Tax ────────────────────────────────────────────────────────────────────
    public decimal TaxPercentage { get; set; } = 14m;

    // ── Taxonomy ───────────────────────────────────────────────────────────────
    public int MaxImagesPerWorkPost { get; set; } = 10;
    public int MaxPackagesPerWorkPost { get; set; } = 5;

    // ── Branding ───────────────────────────────────────────────────────────────
    public string PlatformName { get; set; } = "EventHub";
    public string? PlatformLogoUrl { get; set; }
    public string? SupportEmail { get; set; }

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
