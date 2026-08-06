using EventHub.Domain.Common;

namespace EventHub.Domain.Entities;

/// <summary>
/// Profile data for Customer-role users.
/// PhoneNumber added per audit Module 1 (RegisterRequest schema update).
/// </summary>
public class CustomerProfile : AuditableEntity
{
    public int UserId { get; set; }

    public string FullName { get; set; } = string.Empty;

    /// <summary>Required by audit Module 1: Add PhoneNumber to RegisterRequest / CustomerProfile.</summary>
    public string? PhoneNumber { get; set; }

    public string? City { get; set; }

    public string? AvatarUrl { get; set; }

    // ─── Navigation Properties ────────────────────────────────────────────────
    public User User { get; set; } = null!;

    public ICollection<Event> Events { get; set; } = new List<Event>();

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    
    public ICollection<Review> Reviews { get; set; } = new List<Review>();


}