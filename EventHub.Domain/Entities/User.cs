using EventHub.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace EventHub.Domain.Entities;

public class User : IdentityUser<int>
{
    public UserRole Role { get; set; }


    // Soft Delete
    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedBy { get; set; }

    //public CustomerProfile? CustomerProfile { get; set; }

    // Email Verification
    public bool IsEmailVerified { get; set; }
    public string? EmailVerificationToken { get; set; }
    public DateTime? EmailVerificationExpiry { get; set; }

    // Account Status
    public bool IsActive { get; set; } = true;

    // Refresh Token
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }

    // MFA (for Admin only)
    public string? MfaSecret { get; set; }
    public bool IsMfaEnabled { get; set; }

    // Navigation Properties
    public CustomerProfile? CustomerProfile { get; set; }
    public VendorProfile? VendorProfile { get; set; }
}