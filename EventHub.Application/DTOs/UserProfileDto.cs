namespace EventHub.Application.DTOs;

public class UserProfileDto
{
    public int Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public string? EmailConfirmationToken { get; set; }
}