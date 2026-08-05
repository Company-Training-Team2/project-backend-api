using EventHub.Domain.Enums;

namespace EventHub.Application.DTOs.Home;

public class UpcomingEventSummaryDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public EventType EventType { get; set; }

    public DateTime TargetDate { get; set; }

    public int DaysRemaining { get; set; }
}
