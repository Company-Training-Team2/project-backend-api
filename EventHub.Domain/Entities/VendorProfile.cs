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

    public bool IsVerified { get; set; } = false;

    public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;

    // ─── Navigation Properties ────────────────────────────────────────────────
    public User User { get; set; } = null!;

    public ICollection<WorkPost> WorkPosts { get; set; } = new List<WorkPost>();
}