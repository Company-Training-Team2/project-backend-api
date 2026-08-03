using System.ComponentModel.DataAnnotations;

namespace EventHub.Application.DTOs;

public class UpdateUserDto
{
    [EmailAddress]
    public string? Email { get; set; }

    public string? CurrentPassword { get; set; }

    [MinLength(8)]
    public string? NewPassword { get; set; }
}