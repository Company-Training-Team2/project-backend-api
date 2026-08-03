using Microsoft.AspNetCore.Identity;
using EventHub.Domain.Enums;

namespace EventHub.Domain.Entities;

public class User : IdentityUser<int>
{
    public UserRole Role { get; set; }

<<<<<<< HEAD
    public CustomerProfile? CustomerProfile { get; set; }

=======
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
>>>>>>> 9c5d494 (feat(auth): complete auth-user-schema (Task 1))
    public VendorProfile? VendorProfile { get; set; }
}