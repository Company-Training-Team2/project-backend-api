using EventHub.Application.DTOs.Auth;

namespace EventHub.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> GoogleAuthAsync(GoogleLoginRequest request);
    Task<AuthResponse> AdminLoginAsync(AdminLoginRequest request);

    /// <summary>Per audit Module 1: OTP-based verification. POST /auth/verify-email { email, code }.</summary>
    Task<bool> VerifyEmailOtpAsync(VerifyEmailOtpRequest request);

    /// <summary>Resends a fresh email-verification OTP for an unverified account. POST /auth/resend-otp { email }.</summary>
    Task ResendEmailOtpAsync(ResendOtpRequest request);

    /// <summary>Per audit Module 1: verify reset OTP before allowing password change.</summary>
    Task<bool> VerifyResetCodeAsync(VerifyResetCodeRequest request);

    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
    Task LogoutAsync(int userId);
    Task ForgotPasswordAsync(ForgotPasswordRequest request);

    /// <summary>Per audit Module 1: corrected reset using email + code (not token field reuse).</summary>
    Task<bool> ResetPasswordAsync(ResetPasswordRequest request);
}