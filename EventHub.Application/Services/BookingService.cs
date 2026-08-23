using System.Security.Claims;
using EventHub.Application.DTOs.Booking;
using EventHub.Application.DTOs.Notification;
using EventHub.Application.Interfaces;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace EventHub.Application.Services;

public class BookingService : IBookingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BookingService(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<BookingDto> CreateAsync(CreateBookingDto dto)
    {
        // BookingController had no [Authorize] and this trusted dto.CustomerId
        // from the request body outright — anyone could create a booking under
        // any customer's name. The real frontend (CreateBookingPayload) never
        // even sends a customerId, so every booking made through the app was
        // silently landing with CustomerId = 0, which also broke
        // HomeService's pending/confirmed booking counts and PaymentService's
        // "my payments" list (both filter on Booking.CustomerId == profile.Id,
        // which a real profile's id — never 0 — could never match). Deriving
        // it from the authenticated caller fixes both the impersonation hole
        // and that silent breakage in one change.
        var customerProfile = await GetCurrentCustomerProfileAsync();

        var eventEntity = await _unitOfWork
            .Repository<Event>()
            .GetByIdAsync(dto.EventId);

        if (eventEntity is null)
            throw new Exception("Event not found.");

        if (eventEntity.CustomerId != customerProfile.Id)
            throw new UnauthorizedAccessException("This event does not belong to you.");

        var workPost = await _unitOfWork
            .Repository<WorkPost>()
            .GetByIdAsync(dto.WorkPostId);

        if (workPost is null)
            throw new Exception("Work post not found.");

        var availability = await _unitOfWork
            .Repository<WorkPostAvailability>()
            .FirstOrDefaultAsync(x =>
                x.WorkPostId == dto.WorkPostId &&
                x.Date == dto.BookingDate);

        // A date is available by default — vendors only ever create a row to
        // *block* a specific date (see WorkPostAvailabilityService), never to
        // opt individual dates in. Treating "no row" as unavailable required
        // every bookable date to be pre-created by the vendor, which none of
        // them do; every real booking through ReserveScreen's free-pick
        // calendar was failing here with "Selected date is not available."
        // Only an explicit IsAvailable = false row blocks a booking now.
        if (availability is not null && !availability.IsAvailable)
            throw new Exception("Selected date is not available.");

        decimal totalPrice = workPost.Price * dto.Quantity;

        var booking = new Booking
        {
            CustomerId = customerProfile.Id,
            EventId = dto.EventId,
            WorkPostId = dto.WorkPostId,
            BookingDate = dto.BookingDate,
            Quantity = dto.Quantity,
            Notes = dto.Notes,
            TotalPrice = totalPrice,
            Status = BookingStatus.Pending
        };

        await _unitOfWork
            .Repository<Booking>()
            .AddAsync(booking);

        await _unitOfWork.SaveChangesAsync();

        return MapToDto(booking);
    }

    public async Task<BookingDto> AcceptAsync(int bookingId)
    {
        var booking = await GetBookingWithWorkPostAsync(bookingId);
        await EnsureCurrentUserOwnsWorkPostAsync(booking.WorkPost);

        if (booking.Status != BookingStatus.Pending)
            throw new Exception("Only pending bookings can be accepted.");

        booking.Status = BookingStatus.Accepted;

        _unitOfWork
            .Repository<Booking>()
            .Update(booking);

        await _unitOfWork.SaveChangesAsync();

        await NotifyCustomerAsync(
            booking.CustomerId,
            "Booking confirmed",
            "Your booking has been accepted by the vendor.",
            booking.Id);

        return MapToDto(booking);
    }

    public async Task<BookingDto> RejectAsync(int bookingId)
    {
        var booking = await GetBookingWithWorkPostAsync(bookingId);
        await EnsureCurrentUserOwnsWorkPostAsync(booking.WorkPost);

        if (booking.Status != BookingStatus.Pending)
            throw new Exception("Only pending bookings can be rejected.");

        booking.Status = BookingStatus.Rejected;

        _unitOfWork
            .Repository<Booking>()
            .Update(booking);

        var availability = await _unitOfWork
            .Repository<WorkPostAvailability>()
            .FirstOrDefaultAsync(x =>
                x.WorkPostId == booking.WorkPostId &&
                x.Date == booking.BookingDate);

        if (availability is not null)
        {
            availability.IsAvailable = true;

            _unitOfWork
                .Repository<WorkPostAvailability>()
                .Update(availability);
        }

        await _unitOfWork.SaveChangesAsync();

        await NotifyCustomerAsync(
            booking.CustomerId,
            "Booking rejected",
            "Your booking request was rejected by the vendor.",
            booking.Id);

        return MapToDto(booking);
    }

    public async Task<BookingDto> CancelAsync(int bookingId)
    {
        var booking = await GetBookingWithWorkPostAsync(bookingId);

        // Either side of the booking may cancel it (customer via "My Bookings",
        // vendor via the vendor portal) — Admin too, for support intervention.
        var customerProfile = await GetCurrentCustomerProfileOrNullAsync();
        var isOwningCustomer = customerProfile is not null && booking.CustomerId == customerProfile.Id;

        if (!isOwningCustomer && !IsCurrentUserAdmin())
        {
            var vendorProfile = await GetCurrentVendorProfileOrNullAsync();
            var isOwningVendor = vendorProfile is not null && booking.WorkPost.VendorProfileId == vendorProfile.Id;
            if (!isOwningVendor)
                throw new UnauthorizedAccessException("This booking does not belong to you.");
        }

        if (booking.Status != BookingStatus.Pending &&
            booking.Status != BookingStatus.Accepted)
        {
            throw new Exception("Only pending or confirmed bookings can be cancelled.");
        }

        booking.Status = BookingStatus.Cancelled;

        _unitOfWork
            .Repository<Booking>()
            .Update(booking);

        var availability = await _unitOfWork
            .Repository<WorkPostAvailability>()
            .FirstOrDefaultAsync(x =>
                x.WorkPostId == booking.WorkPostId &&
                x.Date == booking.BookingDate);

        if (availability is not null)
        {
            availability.IsAvailable = true;

            _unitOfWork
                .Repository<WorkPostAvailability>()
                .Update(availability);
        }

        await _unitOfWork.SaveChangesAsync();

        await NotifyCustomerAsync(
            booking.CustomerId,
            "Booking cancelled",
            "Your booking has been cancelled.",
            booking.Id);

        return MapToDto(booking);
    }

    public async Task<BookingDto> GetByIdAsync(int bookingId)
    {
        var booking = await GetBookingWithWorkPostAsync(bookingId);

        var customerProfile = await GetCurrentCustomerProfileOrNullAsync();
        var isOwningCustomer = customerProfile is not null && booking.CustomerId == customerProfile.Id;

        if (!isOwningCustomer && !IsCurrentUserAdmin())
        {
            var vendorProfile = await GetCurrentVendorProfileOrNullAsync();
            var isOwningVendor = vendorProfile is not null && booking.WorkPost.VendorProfileId == vendorProfile.Id;
            if (!isOwningVendor)
                throw new UnauthorizedAccessException("This booking does not belong to you.");
        }

        return MapToDto(booking);
    }

    public async Task<IEnumerable<BookingDto>> GetCustomerBookingsAsync(int customerId, BookingStatus? status = null)
    {
        // Was reachable with any/no customerId — anyone could list any
        // customer's full booking history. Only that customer or an Admin may.
        if (!IsCurrentUserAdmin())
        {
            var customerProfile = await GetCurrentCustomerProfileAsync();
            if (customerProfile.Id != customerId)
                throw new UnauthorizedAccessException("You can only view your own bookings.");
        }

        // Filters on Booking's own CustomerId directly now (correctly
        // populated since CreateAsync started deriving it from the
        // authenticated caller — see its comment) rather than joining
        // through Event.CustomerId as before. That join meant a booking
        // vanished from this list the moment its Event was soft-deleted
        // (EventId is now nullable/optional for exactly this reason) —
        // going through Booking.CustomerId sidesteps Event entirely, so a
        // booking's visibility here no longer depends on its event still
        // existing.
        var bookings = await _unitOfWork
            .Repository<Booking>()
            .FindAsync(b => b.CustomerId == customerId && (status == null || b.Status == status));

        return bookings.Select(MapToDto);
    }

    public async Task<IEnumerable<BookingDto>> GetVendorBookingsAsync(int vendorId, BookingStatus? status = null)
    {
        // Same class of hole as GetCustomerBookingsAsync, vendor-side.
        if (!IsCurrentUserAdmin())
        {
            var vendorProfile = await GetCurrentVendorProfileAsync();
            if (vendorProfile.Id != vendorId)
                throw new UnauthorizedAccessException("You can only view your own bookings.");
        }

        var bookings = await _unitOfWork
            .Repository<Booking>()
            .FindWithIncludeAsync(
                b => b.WorkPost.VendorProfileId == vendorId && (status == null || b.Status == status),
                b => b.WorkPost);

        return bookings.Select(MapToDto);
    }

    // ═══════════════════════════════════════════════════════════
    // Ownership helpers — BookingController has no [Authorize(Roles=...)] of
    // its own (a booking involves both a Customer and a Vendor, so unlike
    // VendorController/AdminController there's no single role to gate the
    // whole controller on); every action here instead resolves the caller's
    // identity and checks it against the booking/customerId/vendorId in
    // question, same pattern as FavoriteService/HomeService's
    // GetCurrent*ProfileAsync helpers.
    // ═══════════════════════════════════════════════════════════

    private int GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?
            .User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("User is not authenticated.");

        return userId;
    }

    private bool IsCurrentUserAdmin() =>
        _httpContextAccessor.HttpContext?.User.IsInRole("Admin") ?? false;

    private async Task<CustomerProfile> GetCurrentCustomerProfileAsync()
    {
        var profile = await GetCurrentCustomerProfileOrNullAsync();
        if (profile is null)
            throw new UnauthorizedAccessException("No customer profile for this account.");
        return profile;
    }

    private async Task<CustomerProfile?> GetCurrentCustomerProfileOrNullAsync()
    {
        var userId = GetCurrentUserId();
        return await _unitOfWork.Repository<CustomerProfile>()
            .FirstOrDefaultAsync(p => p.UserId == userId);
    }

    private async Task<VendorProfile> GetCurrentVendorProfileAsync()
    {
        var profile = await GetCurrentVendorProfileOrNullAsync();
        if (profile is null)
            throw new UnauthorizedAccessException("No vendor profile for this account.");
        return profile;
    }

    private async Task<VendorProfile?> GetCurrentVendorProfileOrNullAsync()
    {
        var userId = GetCurrentUserId();
        return await _unitOfWork.Repository<VendorProfile>()
            .FirstOrDefaultAsync(p => p.UserId == userId);
    }

    private async Task<Booking> GetBookingWithWorkPostAsync(int bookingId)
    {
        var booking = (await _unitOfWork.Repository<Booking>()
            .FindWithIncludeAsync(b => b.Id == bookingId, b => b.WorkPost))
            .FirstOrDefault();

        if (booking is null)
            throw new Exception("Booking not found.");

        return booking;
    }

    private async Task EnsureCurrentUserOwnsWorkPostAsync(WorkPost workPost)
    {
        if (IsCurrentUserAdmin())
            return;

        var vendorProfile = await GetCurrentVendorProfileAsync();
        if (workPost.VendorProfileId != vendorProfile.Id)
            throw new UnauthorizedAccessException("This booking does not belong to you.");
    }

    /// <summary>
    /// Event-driven publishing hook for Module 10: resolves the User behind a
    /// CustomerProfile and raises a BookingStatusUpdate notification (persisted
    /// + pushed via SignalR). Best-effort — a missing profile shouldn't fail
    /// the booking action that triggered it.
    /// </summary>
    private async Task NotifyCustomerAsync(int customerProfileId, string title, string body, int bookingId)
    {
        var profile = await _unitOfWork
            .Repository<CustomerProfile>()
            .GetByIdAsync(customerProfileId);

        if (profile is null)
            return;

        await _notificationService.NotifyAsync(new CreateNotificationDto
        {
            UserId = profile.UserId,
            Type = NotificationType.BookingStatusUpdate,
            Title = title,
            Body = body,
            RelatedEntityId = bookingId
        });
    }

    private BookingDto MapToDto(Booking booking)
    {
        return new BookingDto
        {
            Id = booking.Id,
            // 0 = this booking's event was later soft-deleted; the booking
            // itself (and its payment/expense history) is still real and
            // intact — see Booking.EventId's doc comment. Kept as a plain
            // int on the wire (not int?) so existing consumers don't need
            // to change; 0 is never a real event id.
            EventId = booking.EventId ?? 0,
            WorkPostId = booking.WorkPostId,
            BookingDate = booking.BookingDate,
            Status = booking.Status,
            TotalPrice = booking.TotalPrice,
            Quantity = booking.Quantity,
            Notes = booking.Notes
        };
    }
}