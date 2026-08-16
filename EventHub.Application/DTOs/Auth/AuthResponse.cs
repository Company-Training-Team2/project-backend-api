using EventHub.Domain.Enums;

namespace EventHub.Application.DTOs.Auth;

public class AuthResponse
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string Email { get; set; } = string.Empty;

    /// <summary>Display name — CustomerProfile.FullName or VendorProfile.BusinessName.
    /// Empty on responses with no resolvable profile (e.g. RequiresMfa replies).</summary>
    public string Name { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;
    public bool RequiresMfa { get; set; }

    /// <summary>
    /// REG-CUS-022: Set to <c>true</c> on login responses where the user has not
    /// yet verified their email.  The frontend must use this flag — not the
    /// forgot-password flow — to navigate to the email-verification / OTP screen.
    /// This keeps the Email Verification and Forgot Password flows in separate
    /// navigation contexts so they never bleed into one another.
    /// </summary>
    public bool RequiresEmailVerification { get; set; }
}
