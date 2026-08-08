using System.ComponentModel.DataAnnotations;

namespace EventHub.Application.DTOs.User;

/// <summary>PUT /api/users/me — update profile and/or credentials.</summary>
public class UpdateUserDto
{
    // ─── Profile fields ───────────────────────────────────────────────────────
    public string? FullName { get; set; }

    [Phone]
    public string? PhoneNumber { get; set; }

    public string? City { get; set; }

    public string? AvatarUrl { get; set; }

    // ─── Credential update (both required together if changing password) ──────
    [EmailAddress]
    public string? Email { get; set; }

    public string? CurrentPassword { get; set; }

    [MinLength(8)]
    public string? NewPassword { get; set; }

    [Compare(nameof(NewPassword))]
    public string? ConfirmNewPassword { get; set; }
}