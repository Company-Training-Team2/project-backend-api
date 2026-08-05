namespace EventHub.Application.DTOs;

/// <summary>A single entry in the user's account activity log (GET /api/users/me/activity).</summary>
public class UserActivityDto
{
    public string Action { get; set; } = string.Empty;    // e.g. "Login", "PasswordChanged"
    public string? Detail { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}