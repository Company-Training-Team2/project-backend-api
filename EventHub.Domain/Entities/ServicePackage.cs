using EventHub.Domain.Common;

namespace EventHub.Domain.Entities;

/// <summary>
/// A tiered pricing package offered within a WorkPost.
/// Per audit Module 2: "ServicePackage entity to support tiered pricing models
/// per vendor service (replacing single-price constraints)."
/// </summary>
public class ServicePackage : AuditableEntity
{
    public int WorkPostId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    /// <summary>What is included in this package (e.g. "4 hours + 200 photos").</summary>
    public string? Includes { get; set; }

    public bool IsActive { get; set; } = true;

    // ─── Navigation ───────────────────────────────────────────────────────────
    public WorkPost WorkPost { get; set; } = null!;
}