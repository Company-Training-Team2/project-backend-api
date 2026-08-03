using Microsoft.AspNetCore.Identity;
using EventHub.Domain.Enums;

namespace EventHub.Domain.Entities;

public class User : IdentityUser<int>
{
    public UserRole Role { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public CustomerProfile? CustomerProfile { get; set; }

    public VendorProfile? VendorProfile { get; set; }
}