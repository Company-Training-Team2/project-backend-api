using System.ComponentModel.DataAnnotations;

namespace EventHub.Application.DTOs.Auth;

/// <summary>
/// Per audit Module 1: Migrate email verification from link tokens to a
/// 6-digit OTP code system. Endpoint: POST /auth/verify-email { email, code }.
/// </summary>
public class VerifyEmailOtpRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(6, MinimumLength = 6)]
    public string Code { get; set; } = string.Empty;
}