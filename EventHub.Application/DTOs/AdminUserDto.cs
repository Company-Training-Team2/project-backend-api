namespace EventHub.Application.DTOs;

public class AdminUserDto
{
    public int Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }
}