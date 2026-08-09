namespace EventHub.Application.DTOs.User;

/// <summary>
/// Returned by GET /api/users/me — real data, no mock fields.
/// Per audit Module 12: high-priority refactor of /api/users/me mock data issue.
/// </summary>
public class UserProfileDto
{
    public int Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public bool IsEmailVerified { get; set; }

    public bool IsActive { get; set; }

    // ─── Customer profile fields (null for Vendor/Admin) ──────────────────────
    public string? FullName { get; set; }

    public string? PhoneNumber { get; set; }

    public string? City { get; set; }

    public string? AvatarUrl { get; set; }

    // ─── Vendor profile fields (null for Customer/Admin) ─────────────────────
    public string? BusinessName { get; set; }

    public string? BioDescription { get; set; }

    public string? ApprovalStatus { get; set; }

    public DateTime CreatedAt { get; set; }
}