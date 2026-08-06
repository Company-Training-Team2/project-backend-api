namespace EventHub.Application.DTOs.Booking;

public class CreateBookingDto
{
    public int CustomerId { get; set; }
    public int EventId { get; set; }

    public int WorkPostId { get; set; }

    public DateOnly BookingDate { get; set; }

    public int Quantity { get; set; }

    public string? Notes { get; set; }
}