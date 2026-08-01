using EventHub.Domain.Common;

namespace EventHub.Domain.Entities;

public class CustomerProfile : AuditableEntity
{
    public int UserId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string? City { get; set; }

    // Navigation Properties
    public User User { get; set; } = null!;

    public ICollection<Event> Events { get; set; } = new List<Event>();

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}