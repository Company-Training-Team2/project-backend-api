using EventHub.Domain.Common;

namespace EventHub.Domain.Entities;

public class Event : AuditableEntity
{
    public int CustomerId { get; set; }

    public string EventType { get; set; } = string.Empty;

    public DateTime TargetDate { get; set; }

    public int GuestCount { get; set; }

    public decimal TotalBudget { get; set; }

    public CustomerProfile Customer { get; set; } = null!;

    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}