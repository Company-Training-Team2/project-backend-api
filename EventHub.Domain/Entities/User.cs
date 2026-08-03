using EventHub.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace EventHub.Domain.Entities;

public class User : IdentityUser<int>
{
    public UserRole Role { get; set; }

    // Soft Delete
    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedBy { get; set; }

    public CustomerProfile? CustomerProfile { get; set; }

    public VendorProfile? VendorProfile { get; set; }
}