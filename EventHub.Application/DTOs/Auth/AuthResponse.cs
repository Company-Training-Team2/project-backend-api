using EventHub.Domain.Enums;

namespace EventHub.Application.DTOs.Auth;

public class AuthResponse
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string Email { get; set; } = string.Empty;

    /// <summary>Display name — CustomerProfile.FullName or VendorProfile.BusinessName.
    /// Empty on responses with no resolvable profile (e.g. RequiresMfa replies).</summary>
    public string Name { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;
    public bool RequiresMfa { get; set; }
}
