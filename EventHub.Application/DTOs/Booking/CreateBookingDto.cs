namespace EventHub.Application.DTOs.Booking;

public class CreateBookingDto
{
    // CustomerId used to live here and be trusted straight from the request
    // body — with BookingController wide open (no [Authorize]), that let
    // anyone book as any customer. It's now derived server-side from the
    // authenticated caller's own CustomerProfile (see BookingService.
    // CreateAsync) instead of being client-supplied.
    public int EventId { get; set; }

    public int WorkPostId { get; set; }

    public DateOnly BookingDate { get; set; }

    public int Quantity { get; set; }

    public string? Notes { get; set; }
}