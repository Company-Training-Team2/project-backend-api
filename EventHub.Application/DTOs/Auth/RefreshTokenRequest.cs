using System.ComponentModel.DataAnnotations;

namespace EventHub.Application.DTOs.Auth;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
