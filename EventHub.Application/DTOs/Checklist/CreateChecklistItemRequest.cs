using System.ComponentModel.DataAnnotations;

namespace EventHub.Application.DTOs.Checklist;

/// <summary>
/// Request body for POST /api/events/{id}/checklist.
/// </summary>
public class CreateChecklistItemRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    /// <summary>Accepted values: Low, Medium, High. Defaults to Medium.</summary>
    public string Priority { get; set; } = "Medium";

    [MaxLength(100)]
    public string? Category { get; set; }
}