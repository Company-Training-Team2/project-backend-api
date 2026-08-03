using EventHub.Domain.Enums;

namespace EventHub.Application.DTOs.Booking;

public class BookingDto
{
    public int Id { get; set; }

    public int EventId { get; set; }

    public int WorkPostId { get; set; }

    public DateOnly BookingDate { get; set; }

    public BookingStatus Status { get; set; }

    public decimal TotalPrice { get; set; }

    public int Quantity { get; set; }

    public string? Notes { get; set; }
}