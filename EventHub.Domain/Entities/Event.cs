using EventHub.Domain.Common;
using EventHub.Domain.Enums;

namespace EventHub.Domain.Entities;

public class Event : SoftDeletableEntity
{
    public int CustomerId { get; set; }

    public EventType EventType { get; set; }

    public DateTime TargetDate { get; set; }

    public int GuestCount { get; set; }

    public decimal TotalBudget { get; set; }

    public string? Notes { get; set; }

    // Navigation Properties
    public CustomerProfile Customer { get; set; } = null!;

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}