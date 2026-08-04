using System.ComponentModel.DataAnnotations;
using EventHub.Domain.Enums;

namespace EventHub.Application.DTOs;

public class RegisterRequest
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;

    [Required, Compare("Password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    public UserRole Role { get; set; } // Customer or Vendor only

    // Customer fields
    public string? FullName { get; set; }
    public string? City { get; set; }

    // Vendor fields
    public string? BusinessName { get; set; }
    public string? BioDescription { get; set; }
}
