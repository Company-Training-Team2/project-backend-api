using System.ComponentModel.DataAnnotations;

namespace EventHub.Application.DTOs.Checklist;

public class UpdateChecklistItemRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    /// <summary>Accepted values: Low, Medium, High.</summary>
    public string Priority { get; set; } = "Medium";

    public bool IsCompleted { get; set; }

    [MaxLength(100)]
    public string? Category { get; set; }
}