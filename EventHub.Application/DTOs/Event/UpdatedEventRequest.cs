using EventHub.Domain.Enums;

namespace EventHub.Application.DTOs.Event;

public class UpdateEventRequest
{
    public string Name { get; set; } = string.Empty;
    public EventType EventType { get; set; }
    public DateTime TargetDate { get; set; }
    public int GuestCount { get; set; }
    public decimal TotalBudget { get; set; }
    public string City { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? Notes { get; set; }
}
