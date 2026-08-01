using EventHub.Domain.Common;

namespace EventHub.Domain.Entities;

public class Review : AuditableEntity
{
    public int BookingId { get; set; }

    public int Rating { get; set; }

    public string? Comment { get; set; }

    // Navigation Property
    public Booking Booking { get; set; } = null!;
}