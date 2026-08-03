namespace EventHub.Application.DTOs.Booking.WorkPostAvailability;

public class CreateWorkPostAvailabilityDto
{
    public int WorkPostId { get; set; }

    public DateOnly Date { get; set; }

    public bool IsAvailable { get; set; }

    public string? Notes { get; set; }
}