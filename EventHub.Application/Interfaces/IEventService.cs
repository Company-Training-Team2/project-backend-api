using EventHub.Application.DTOs;

namespace EventHub.Application.Interfaces;

public interface IEventService
{
    Task<EventResponse> CreateEventAsync(
        int userId,
        CreateEventRequest request);

    Task<IEnumerable<EventResponse>> GetUserEventsAsync(
        int userId);

    Task<EventResponse?> GetEventByIdAsync(
        int eventId,
        int userId);

    Task<EventResponse?> UpdateEventAsync(
        int eventId,
        int userId,
        UpdateEventRequest request);

    Task<bool> DeleteEventAsync(
        int eventId,
        int userId);

    Task<EventDashboardResponse?> GetEventDashboardAsync(
        int eventId,
        int userId);

    Task<IEnumerable<EventVendorResponse>> GetEventVendorsAsync(
        int eventId,
        int userId);

    /// <summary>
    /// True if the event exists and belongs to the CustomerProfile linked to this
    /// (Identity) userId. Shared by other services (e.g. GuestService) that need
    /// to authorize access to a sub-resource of an event without duplicating the
    /// User -> CustomerProfile resolution logic.
    /// </summary>
    Task<bool> EventBelongsToUserAsync(
        int eventId,
        int userId);
}
