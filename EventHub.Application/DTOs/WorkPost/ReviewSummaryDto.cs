namespace EventHub.Application.DTOs.WorkPost;

public class ReviewSummaryDto
{
    public int Id { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public int Rating { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; }
}
