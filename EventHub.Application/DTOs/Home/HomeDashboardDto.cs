using EventHub.Application.DTOs.WorkPost;

namespace EventHub.Application.DTOs.Home;

/// <summary>
/// Aggregated payload for GET /api/home/dashboard (audit Module 2).
/// Combines the customer's events, bookings, favorites and a
/// recommended-vendors strip into a single call for the Home screen.
/// </summary>
public class HomeDashboardDto
{
    public string CustomerName { get; set; } = string.Empty;

    public int UpcomingEventsCount { get; set; }

    public UpcomingEventSummaryDto? NextEvent { get; set; }

    public int FavoritesCount { get; set; }

    public int PendingBookingsCount { get; set; }

    public int ConfirmedBookingsCount { get; set; }

    public List<WorkPostSummaryDto> RecommendedWorkPosts { get; set; } = new();
}
