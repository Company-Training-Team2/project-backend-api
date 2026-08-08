using System.ComponentModel.DataAnnotations;

namespace EventHub.Application.DTOs.Auth;

public class GoogleLoginRequest
{
    [Required]
    public string IdToken { get; set; } = string.Empty;
}
