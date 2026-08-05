namespace EventHub.Application.DTOs.Platform;

/// <summary>
/// Public / unauthenticated marketing metrics for GET /api/platform/stats
/// (audit Module 2).
/// </summary>
public class PlatformStatsDto
{
    public int TotalVendors { get; set; }

    public int TotalCustomers { get; set; }

    public int TotalApprovedWorkPosts { get; set; }

    public int TotalCategories { get; set; }

    public int TotalCompletedBookings { get; set; }

    public int TotalEventsPlanned { get; set; }

    public double AveragePlatformRating { get; set; }
}
