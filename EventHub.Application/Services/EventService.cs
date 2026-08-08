using EventHub.Application.DTOs.Event;
using EventHub.Application.DTOs.Guest;
using EventHub.Application.Interfaces;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;

namespace EventHub.Application.Services;

/// <summary>
/// Module 3 (Event Management): Customer Events List -> Event Dashboard
/// (Countdown, Budget, Task Velocity, Guest RSVP) -> Event Vendors.
///
/// IMPORTANT: the "userId" parameter throughout this service is the
/// ASP.NET Identity User.Id (from the JWT NameIdentifier claim, resolved once
/// in EventsController.GetCurrentUserId()). Event.CustomerId is a FK to
/// CustomerProfile.Id, which is a *different* value. Every method here
/// resolves User.Id -> CustomerProfile.Id before comparing against
/// Event.CustomerId, mirroring the pattern already used in FavoriteService.
/// </summary>
public class EventService : IEventService
{
    private readonly IEventRepository _eventRepository;
    private readonly IUnitOfWork _unitOfWork;

    public EventService(IEventRepository eventRepository, IUnitOfWork unitOfWork)
    {
        _eventRepository = eventRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<EventResponse> CreateEventAsync(
        int userId,
        CreateEventRequest request)
    {
        var profile = await GetCustomerProfileAsync(userId)
            ?? throw new InvalidOperationException("Customer profile not found for the current user.");

        var eventEntity = new Event
        {
            CustomerId = profile.Id,
            Name = request.Name,
            EventType = request.EventType,
            TargetDate = request.TargetDate,
            GuestCount = request.GuestCount,
            TotalBudget = request.TotalBudget,
            City = request.City,
            Location = request.Location,
            Notes = request.Notes
        };

        await _eventRepository.AddAsync(eventEntity);
        await _eventRepository.SaveChangesAsync();

        return MapToResponse(eventEntity);
    }

    public async Task<IEnumerable<EventResponse>> GetUserEventsAsync(int userId)
    {
        var profile = await GetCustomerProfileAsync(userId);
        if (profile is null)
            return Enumerable.Empty<EventResponse>();

        var events = await _eventRepository.GetByUserIdAsync(profile.Id);

        return events.Select(MapToResponse);
    }

    public async Task<EventResponse?> GetEventByIdAsync(
        int eventId,
        int userId)
    {
        var profile = await GetCustomerProfileAsync(userId);
        if (profile is null)
            return null;

        var evt = await _eventRepository.GetByIdAsync(eventId);

        if (evt == null || evt.CustomerId != profile.Id)
            return null;

        return MapToResponse(evt);
    }

    public async Task<EventResponse?> UpdateEventAsync(
        int eventId,
        int userId,
        UpdateEventRequest request)
    {
        var profile = await GetCustomerProfileAsync(userId);
        if (profile is null)
            return null;

        var evt = await _eventRepository.GetByIdAsync(eventId);

        if (evt == null || evt.CustomerId != profile.Id)
            return null;

        evt.Name = request.Name;
        evt.EventType = request.EventType;
        evt.TargetDate = request.TargetDate;
        evt.GuestCount = request.GuestCount;
        evt.TotalBudget = request.TotalBudget;
        evt.City = request.City;
        evt.Location = request.Location;
        evt.Notes = request.Notes;

        await _eventRepository.SaveChangesAsync();

        return MapToResponse(evt);
    }

    public async Task<bool> DeleteEventAsync(
        int eventId,
        int userId)
    {
        var profile = await GetCustomerProfileAsync(userId);
        if (profile is null)
            return false;

        var evt = await _eventRepository.GetByIdAsync(eventId);

        if (evt == null || evt.CustomerId != profile.Id)
            return false;

        await _eventRepository.Remove(eventId);
        await _eventRepository.SaveChangesAsync();

        return true;
    }

    public async Task<EventDashboardResponse?> GetEventDashboardAsync(
        int eventId,
        int userId)
    {
        var profile = await GetCustomerProfileAsync(userId);
        if (profile is null)
            return null;

        var evt = await _eventRepository.GetByIdWithDashboardDataAsync(eventId);

        if (evt == null || evt.CustomerId != profile.Id)
            return null;

        var totalTasks = evt.ChecklistItems.Count;
        var completedTasks = evt.ChecklistItems.Count(c => c.IsCompleted);

        var spentBudget = evt.Expenses
            .Where(e => e.Status == ExpenseStatus.Paid)
            .Sum(e => e.Amount);

        // Ceiling so "1 day and a few hours away" still counts as 1, not 0.
        var daysUntilEvent = (int)Math.Ceiling(
            (evt.TargetDate.Date - DateTime.UtcNow.Date).TotalDays);

        return new EventDashboardResponse
        {
            EventId = evt.Id,
            Name = evt.Name,
            DaysUntilEvent = daysUntilEvent,
            TotalBudget = evt.TotalBudget,
            SpentBudget = spentBudget,
            RemainingBudget = evt.TotalBudget - spentBudget,
            TotalTasks = totalTasks,
            CompletedTasks = completedTasks,
            PendingTasks = totalTasks - completedTasks,
            ConfirmedGuests = evt.Guests.Count(g => g.RSVPStatus == RSVPStatus.Confirmed),
            PendingGuests = evt.Guests.Count(g => g.RSVPStatus == RSVPStatus.Pending),
            DeclinedGuests = evt.Guests.Count(g => g.RSVPStatus == RSVPStatus.Declined)
        };
    }

    public async Task<IEnumerable<EventVendorResponse>> GetEventVendorsAsync(
        int eventId,
        int userId)
    {
        var profile = await GetCustomerProfileAsync(userId);
        if (profile is null)
            return Enumerable.Empty<EventVendorResponse>();

        var evt = await _eventRepository.GetByIdWithVendorsAsync(eventId);

        if (evt == null || evt.CustomerId != profile.Id)
            return Enumerable.Empty<EventVendorResponse>();

        return evt.Bookings
            .OrderByDescending(b => b.BookingDate)
            .Select(b => new EventVendorResponse
            {
                BookingId = b.Id,
                VendorProfileId = b.WorkPost.VendorProfileId,
                VendorName = b.WorkPost.VendorProfile.BusinessName,
                ServiceTitle = b.WorkPost.Title,
                BookingStatus = b.Status.ToString(),
                Amount = b.TotalPrice,
                BookingDate = b.BookingDate.ToDateTime(TimeOnly.MinValue)
            })
            .ToList();
    }

    public async Task<bool> EventBelongsToUserAsync(int eventId, int userId)
    {
        var profile = await GetCustomerProfileAsync(userId);
        if (profile is null)
            return false;

        var evt = await _eventRepository.GetByIdAsync(eventId);

        return evt != null && evt.CustomerId == profile.Id;
    }

    private async Task<CustomerProfile?> GetCustomerProfileAsync(int userId)
    {
        return await _unitOfWork.Repository<CustomerProfile>()
            .FirstOrDefaultAsync(p => p.UserId == userId);
    }

    private static EventResponse MapToResponse(Event evt)
    {
        return new EventResponse
        {
            Id = evt.Id,
            CustomerId = evt.CustomerId,
            Name = evt.Name,
            EventType = evt.EventType.ToString(),
            TargetDate = evt.TargetDate,
            GuestCount = evt.GuestCount,
            TotalBudget = evt.TotalBudget,
            City = evt.City,
            Location = evt.Location,
            Notes = evt.Notes,
            CreatedAt = evt.CreatedAt,
            UpdatedAt = evt.UpdatedAt
        };
    }
}
