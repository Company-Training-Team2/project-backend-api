using EventHub.Application.DTOs;

namespace EventHub.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> GoogleAuthAsync(GoogleLoginRequest request);
    Task<AuthResponse> AdminLoginAsync(AdminLoginRequest request);
    Task<bool> VerifyEmailAsync(string token);
    Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
    Task LogoutAsync(int userId);
    Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<bool> ResetPasswordAsync(ResetPasswordRequest request);
}
