using System.ComponentModel.DataAnnotations;

namespace EventHub.Application.DTOs.Auth;

/// <summary>
/// Per audit Module 1: POST /auth/verify-reset-code — verify the OTP code
/// before allowing password reset.
/// </summary>
public class VerifyResetCodeRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(6, MinimumLength = 6)]
    public string Code { get; set; } = string.Empty;
}