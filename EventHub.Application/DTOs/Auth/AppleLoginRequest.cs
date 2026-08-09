using System.ComponentModel.DataAnnotations;

namespace EventHub.Application.DTOs.Auth;

public class AppleLoginRequest
{
    /// <summary>The id_token Apple's JS SDK hands back to the browser after a
    /// successful "Sign in with Apple" popup — same shape/role as
    /// GoogleLoginRequest.IdToken.</summary>
    [Required]
    public string IdToken { get; set; } = string.Empty;
}
