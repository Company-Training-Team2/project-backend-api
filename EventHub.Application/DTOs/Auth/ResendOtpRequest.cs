using System.ComponentModel.DataAnnotations;

namespace EventHub.Application.DTOs.Auth;

/// <summary>Resend the 6-digit email-verification OTP. POST /auth/resend-otp { email }.</summary>
public class ResendOtpRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}
