namespace EventHub.Application.DTOs.Admin;

/// <summary>User record as seen by Admin in GET /api/admin/users.</summary>
public class AdminUserDto
{
    public int Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    public bool IsEmailVerified { get; set; }

    public DateTime CreatedAt { get; set; }

    // Profile snapshot
    public string? FullName { get; set; }      // Customer
    public string? BusinessName { get; set; }  // Vendor
}