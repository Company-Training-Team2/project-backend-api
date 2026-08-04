using System.ComponentModel.DataAnnotations;

namespace EventHub.Application.DTOs;

public class RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
