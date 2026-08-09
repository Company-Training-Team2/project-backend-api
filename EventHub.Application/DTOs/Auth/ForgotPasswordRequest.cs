using System.ComponentModel.DataAnnotations;

namespace EventHub.Application.DTOs.Auth;

public class ForgotPasswordRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}
