using EventHub.Application.DTOs.Guest;
using EventHub.Application.Interfaces;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;

namespace EventHub.Application.Services;

/// <summary>
/// Backs the RSVP list widget ("Confirmed 124 / Pending 42 / Declined 8") on the
/// Event Dashboard. Every method authorizes via IEventService.EventBelongsToUserAsync
/// so a guest can only ever be read/mutated by the customer who owns the parent event.
/// </summary>
public class GuestService : IGuestService
{
    private readonly IGuestRepository _guestRepository;
    private readonly IEventService _eventService;

    public GuestService(IGuestRepository guestRepository, IEventService eventService)
    {
        _guestRepository = guestRepository;
        _eventService = eventService;
    }

    public async Task<GuestResponse?> AddGuestAsync(int eventId, int userId, CreateGuestRequest request)
    {
        var owned = await _eventService.EventBelongsToUserAsync(eventId, userId);
        if (!owned)
            return null;

        var guest = new Guest
        {
            EventId = eventId,
            Name = request.Name,
            Email = request.Email,
            PhoneNumber = request.Phone,
            RSVPStatus = RSVPStatus.Pending
        };

        await _guestRepository.AddAsync(guest);
        await _guestRepository.SaveChangesAsync();

        return MapToResponse(guest);
    }

    public async Task<IEnumerable<GuestResponse>> GetEventGuestsAsync(int eventId, int userId)
    {
        var owned = await _eventService.EventBelongsToUserAsync(eventId, userId);
        if (!owned)
            return Enumerable.Empty<GuestResponse>();

        var guests = await _guestRepository.GetByEventIdAsync(eventId);
        return guests.Select(MapToResponse);
    }

    public async Task<bool> UpdateRSVPStatusAsync(int guestId, int userId, string status)
    {
        var guest = await _guestRepository.GetByIdWithEventAsync(guestId);
        if (guest == null)
            return false;

        var owned = await _eventService.EventBelongsToUserAsync(guest.EventId, userId);
        if (!owned)
            return false;

        if (!Enum.TryParse<RSVPStatus>(status, true, out var rsvpStatus))
            return false;

        guest.RSVPStatus = rsvpStatus;
        await _guestRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveGuestAsync(int guestId, int userId)
    {
        var guest = await _guestRepository.GetByIdWithEventAsync(guestId);
        if (guest == null)
            return false;

        var owned = await _eventService.EventBelongsToUserAsync(guest.EventId, userId);
        if (!owned)
            return false;

        _guestRepository.Remove(guest);
        await _guestRepository.SaveChangesAsync();
        return true;
    }

    private static GuestResponse MapToResponse(Guest guest)
    {
        return new GuestResponse
        {
            Id = guest.Id,
            EventId = guest.EventId,
            Name = guest.Name,
            Email = guest.Email,
            Phone = guest.PhoneNumber,
            RSVPStatus = guest.RSVPStatus.ToString()
        };
    }
}
