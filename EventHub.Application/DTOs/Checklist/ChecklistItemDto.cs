namespace EventHub.Application.DTOs.Checklist;

/// <summary>
/// Full checklist item payload returned by GET and POST.
/// </summary>
public class ChecklistItemDto
{
    public int Id { get; set; }
    public int EventId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime? DueDate { get; set; }

    /// <summary>Low | Medium | High</summary>
    public string Priority { get; set; } = string.Empty;

    public bool IsCompleted { get; set; }

    /// <summary>Vendor category link (e.g. "Catering", "Photography").</summary>
    public string? Category { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}