using EventHub.Domain.Common;

namespace EventHub.Domain.Entities;

public class WorkPostAvailability : BaseEntity
{
    public int WorkPostId { get; set; }

    public DateOnly Date { get; set; }

    public bool IsAvailable { get; set; } = true;

    public string? Notes { get; set; }

    public WorkPost WorkPost { get; set; } = null!;
}