namespace EventHub.Domain.Constants;

/// <summary>
/// Domain-level constants for authentication and token lifecycle.
/// All magic numbers that were previously scattered inline in AuthService
/// are now declared here so they can be updated in one place.
/// </summary>
public static class AuthConstants
{
    // ── OTP ────────────────────────────────────────────────────────────
    /// <summary>Number of minutes before an email-verification OTP expires.</summary>
    public const int EmailOtpExpiryMinutes = 15;

    /// <summary>Number of minutes before a password-reset OTP expires.</summary>
    public const int PasswordResetOtpExpiryMinutes = 15;

    /// <summary>Inclusive lower bound for the 6-digit OTP code (100 000).</summary>
    public const int OtpMinValue = 100_000;

    /// <summary>Exclusive upper bound for the 6-digit OTP code (999 999).</summary>
    public const int OtpMaxValue = 999_999;

    /// <summary>Minimum seconds a user must wait between "resend OTP" requests.</summary>
    public const int ResendOtpCooldownSeconds = 60;

    // ── Refresh token ───────────────────────────────────────────────────
    /// <summary>Number of days a refresh token remains valid.</summary>
    public const int RefreshTokenExpiryDays = 7;

    // ── Messages ────────────────────────────────────────────────────────
    public const string RegistrationSuccessMessage =
        "Registration successful. Please check your email for the 6-digit verification code.";

    public const string LoginSuccessMessage = "Login successful.";
    public const string AdminLoginSuccessMessage = "Admin login successful.";
    public const string GoogleLoginSuccessMessage = "Google login successful.";
    public const string TokenRefreshedMessage = "Token refreshed.";
    public const string MfaRequiredMessage = "MFA required.";
    public const string AdminMfaRequiredMessage = "MFA required. Please enter your authenticator code.";

    public const string InvalidCredentialsMessage = "Invalid email or password.";
    public const string AccountLockedOutMessage =
        "Too many failed sign-in attempts. Your account is temporarily locked — try again in a few minutes.";
    public const string AccountDeactivatedMessage = "Account is deactivated.";
    public const string EmailNotVerifiedMessage =
        "Email not verified. Please enter the 6-digit code sent to your inbox.";
    public const string VendorPendingApprovalMessage = "Vendor account is pending admin approval.";
    public const string InvalidRefreshTokenMessage = "Invalid or expired refresh token.";
    public const string AdminRegistrationForbiddenMessage = "Admin registration is not allowed.";
    public const string EmailAlreadyRegisteredMessage = "Email already registered.";
    public const string InvalidAdminCredentialsMessage = "Invalid admin credentials.";
    public const string InvalidGoogleTokenMessage = "Invalid Google token.";
    public const string GoogleUserCreationFailedMessage = "Failed to create user from Google login.";
    public const string AppleLoginSuccessMessage = "Apple login successful.";
    public const string InvalidAppleTokenMessage = "Invalid Apple token.";
    public const string AppleUserCreationFailedMessage = "Failed to create user from Apple login.";
    public const string OtpSendFailedMessage =
        "We couldn't send the verification email. Please try registering again in a moment.";
    public const string AccountNotFoundMessage = "No account found for this email.";
    public const string EmailAlreadyVerifiedMessage = "Email is already verified. Please login.";
    public const string ResendOtpSuccessMessage = "A new verification code has been sent to your email.";

    public static string ResendOtpCooldownMessage(int secondsRemaining) =>
        $"Please wait {secondsRemaining} second(s) before requesting another code.";
}
