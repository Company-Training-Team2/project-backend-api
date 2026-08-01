using EventHub.Domain.Common;

namespace EventHub.Domain.Entities;

public class WorkPostImage : BaseEntity
{
    public int WorkPostId { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    public bool IsPrimary { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    // Navigation Property
    public WorkPost WorkPost { get; set; } = null!;
}