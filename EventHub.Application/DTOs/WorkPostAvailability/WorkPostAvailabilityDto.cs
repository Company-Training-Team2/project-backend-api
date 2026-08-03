namespace EventHub.Application.DTOs.Booking.WorkPostAvailability;

public class WorkPostAvailabilityDto
{
    public int Id { get; set; }

    public int WorkPostId { get; set; }

    public DateOnly Date { get; set; }

    public bool IsAvailable { get; set; }

    public string? Notes { get; set; }
}