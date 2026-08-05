using EventHub.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace EventHub.Domain.Entities;

/// <summary>
/// Core identity entity. Extends ASP.NET Identity with domain-specific fields.
/// Soft-delete via IsDeleted / DeletedAt (mirrors SoftDeletableEntity pattern
/// but cannot inherit it because IdentityUser is already the base class).
/// </summary>
public class User : IdentityUser<int>
{
    // ─── Role ─────────────────────────────────────────────────────────────────
    public UserRole Role { get; set; }

    // ─── Soft Delete ──────────────────────────────────────────────────────────
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public int? DeletedBy { get; set; }

    // ─── Account Status ───────────────────────────────────────────────────────
    public bool IsActive { get; set; } = true;

    // ─── Email Verification (OTP-based, per audit Module 1) ──────────────────
    public bool IsEmailVerified { get; set; } = false;
    /// <summary>6-digit OTP code sent to user email.</summary>
    public string? EmailVerificationCode { get; set; }
    public DateTime? EmailVerificationExpiry { get; set; }

    // ─── Password Reset (audit Module 1: PasswordResetCode) ──────────────────
    public string? PasswordResetCode { get; set; }
    public DateTime? PasswordResetCodeExpiry { get; set; }

    // ─── Refresh Token ────────────────────────────────────────────────────────
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }

    // ─── MFA (Admin only, per audit Module 1) ────────────────────────────────
    public string? MfaSecret { get; set; }
    public bool IsMfaEnabled { get; set; } = false;

    // ─── Audit ────────────────────────────────────────────────────────────────
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // ─── Navigation Properties ────────────────────────────────────────────────
    public CustomerProfile? CustomerProfile { get; set; }
    public VendorProfile? VendorProfile { get; set; }

    public ICollection<Notification> Notifications { get; set; }
    = new List<Notification>();
}