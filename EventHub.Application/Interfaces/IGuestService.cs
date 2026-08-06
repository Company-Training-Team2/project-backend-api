using EventHub.Application.DTOs;

namespace EventHub.Application.Interfaces;

public interface IGuestService
{
    Task<GuestResponse?> AddGuestAsync(int eventId, int userId, CreateGuestRequest request);
    Task<IEnumerable<GuestResponse>> GetEventGuestsAsync(int eventId, int userId);
    Task<bool> UpdateRSVPStatusAsync(int guestId, int userId, string status);
    Task<bool> RemoveGuestAsync(int guestId, int userId);
}
