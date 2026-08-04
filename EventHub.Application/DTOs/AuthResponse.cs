using EventHub.Domain.Enums;

namespace EventHub.Application.DTOs;

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public bool RequiresMfa { get; set; }
}
