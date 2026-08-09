using System.ComponentModel.DataAnnotations;

namespace EventHub.Application.DTOs.Auth;

public class MfaVerifyRequest
{
    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Code { get; set; } = string.Empty;
}
