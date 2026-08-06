namespace EventHub.Application.DTOs.Checklist;

/// <summary>
/// Module 5 checklist payload: tasks grouped by completion status and
/// sorted by priority (High → Medium → Low) within each bucket.
/// </summary>
public class EventChecklistResponse
{
    public int EventId { get; set; }
    public int TotalCount { get; set; }
    public int CompletedCount { get; set; }
    public int PendingCount { get; set; }

    public IEnumerable<ChecklistItemDto> Pending { get; set; }
        = Enumerable.Empty<ChecklistItemDto>();

    public IEnumerable<ChecklistItemDto> Completed { get; set; }
        = Enumerable.Empty<ChecklistItemDto>();
}