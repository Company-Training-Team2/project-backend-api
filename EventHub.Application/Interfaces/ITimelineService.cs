using EventHub.Application.DTOs.Timeline;

namespace EventHub.Application.Interfaces;

/// <summary>
/// Module 6 – Dynamic milestone timeline.
/// Status is computed from Event, Booking, and Payment aggregates —
/// no dedicated DB state table is required.
/// </summary>
public interface ITimelineService
{
    /// <summary>
    /// Evaluates and returns the ordered milestone list for the event.
    /// Returns null when the event is not found or does not belong to the user.
    /// </summary>
    Task<TimelineResponse?> GetTimelineAsync(int eventId, int userId);
}