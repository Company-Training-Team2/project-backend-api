using System.ComponentModel.DataAnnotations;

namespace EventHub.Application.DTOs.User;

/// <summary>PUT /api/users/me/password — dedicated password change endpoint.</summary>
public class ChangePasswordDto
{
    [Required]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string NewPassword { get; set; } = string.Empty;

    [Required, Compare(nameof(NewPassword))]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}