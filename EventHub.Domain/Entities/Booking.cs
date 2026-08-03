using EventHub.Domain.Common;
using EventHub.Domain.Enums;

namespace EventHub.Domain.Entities;

public class Booking : AuditableEntity
{
<<<<<<< HEAD
=======
    public int CustomerId { get; set; }

>>>>>>> 9c5d494 (feat(auth): complete auth-user-schema (Task 1))
    public int EventId { get; set; }

    public int WorkPostId { get; set; }

    public BookingStatus Status { get; set; }

    public decimal TotalPrice { get; set; }

    public int Quantity { get; set; }

    public string? Notes { get; set; }

    // Navigation Properties
<<<<<<< HEAD
=======
    public CustomerProfile Customer { get; set; } = null!;

>>>>>>> 9c5d494 (feat(auth): complete auth-user-schema (Task 1))
    public Event Event { get; set; } = null!;

    public WorkPost WorkPost { get; set; } = null!;

    public Payment? Payment { get; set; }

    public Review? Review { get; set; }
}