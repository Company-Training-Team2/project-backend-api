using System.ComponentModel.DataAnnotations;

namespace EventHub.Application.DTOs.Auth;

/// <summary>
/// Used after /auth/verify-reset-code has confirmed the OTP.
/// Email + Code identify the user; NewPassword is applied.
/// Per audit Module 1: fix bugs in ResetPasswordAsync.
/// </summary>
public class ResetPasswordRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, StringLength(6, MinimumLength = 6)]
    public string Code { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string NewPassword { get; set; } = string.Empty;

    [Required, Compare(nameof(NewPassword))]
    public string ConfirmPassword { get; set; } = string.Empty;
}