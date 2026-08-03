namespace EventHub.Application.DTOs.Booking.WorkPostAvailability;

public class UpdateWorkPostAvailabilityDto
{
    public DateOnly Date { get; set; }

    public bool IsAvailable { get; set; }

    public string? Notes { get; set; }
}