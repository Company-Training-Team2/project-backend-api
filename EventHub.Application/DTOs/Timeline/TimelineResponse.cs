namespace EventHub.Application.DTOs.Timeline;

/// <summary>
/// Module 6 – dynamic milestone timeline computed from Event, Booking,
/// and Payment state aggregates (no dedicated DB table required).
/// </summary>
public class TimelineResponse
{
    public int EventId { get; set; }
    public string EventName { get; set; } = string.Empty;

    /// <summary>
    /// Ordered list of milestones. The first milestone whose
    /// IsCompleted == false is the "current" step.
    /// </summary>
    public IEnumerable<TimelineMilestoneDto> Milestones { get; set; }
        = Enumerable.Empty<TimelineMilestoneDto>();
}

public class TimelineMilestoneDto
{
    /// <summary>
    /// Canonical milestone keys (in order):
    ///   planning_started | vendor_booked | invitation_sent |
    ///   payments_deposits | final_confirmation | event_day | completed
    /// </summary>
    public string Key { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public bool IsCompleted { get; set; }

    /// <summary>
    /// The date/time that triggered this milestone's completion, when
    /// determinable (e.g. first booking date, event TargetDate). Null
    /// for milestones that are not yet reached.
    /// </summary>
    public DateTime? CompletedAt { get; set; }

    public string? Description { get; set; }
}