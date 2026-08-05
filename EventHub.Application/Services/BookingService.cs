using EventHub.Application.DTOs.Booking;
using EventHub.Application.DTOs.Notification;
using EventHub.Application.Interfaces;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;

namespace EventHub.Application.Services;

public class BookingService : IBookingService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;

    public BookingService(IUnitOfWork unitOfWork, INotificationService notificationService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
    }

    public async Task<BookingDto> CreateAsync(CreateBookingDto dto)
    {
        var eventEntity = await _unitOfWork
            .Repository<Event>()
            .GetByIdAsync(dto.EventId);

        if (eventEntity is null)
            throw new Exception("Event not found.");

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

        if (availability is null || !availability.IsAvailable)
            throw new Exception("Selected date is not available.");

        decimal totalPrice = workPost.Price * dto.Quantity;

        var booking = new Booking
        {
            CustomerId = 1, // temporary test customer
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
        var booking = await _unitOfWork
            .Repository<Booking>()
            .GetByIdAsync(bookingId);

        if (booking is null)
            throw new Exception("Booking not found.");

        if (booking.Status != BookingStatus.Pending)
            throw new Exception("Only pending bookings can be accepted.");

        booking.Status = BookingStatus.Confirmed;

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
        var booking = await _unitOfWork
            .Repository<Booking>()
            .GetByIdAsync(bookingId);

        if (booking is null)
            throw new Exception("Booking not found.");

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
        var booking = await _unitOfWork
            .Repository<Booking>()
            .GetByIdAsync(bookingId);

        if (booking is null)
            throw new Exception("Booking not found.");

        if (booking.Status != BookingStatus.Pending &&
            booking.Status != BookingStatus.Confirmed)
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
        var booking = await _unitOfWork
            .Repository<Booking>()
            .GetByIdAsync(bookingId);

        if (booking is null)
            throw new Exception("Booking not found.");

        return MapToDto(booking);
    }

    public async Task<IEnumerable<BookingDto>> GetCustomerBookingsAsync(int customerId)
    {
        var bookings = await _unitOfWork
            .Repository<Booking>()
            .FindWithIncludeAsync(
                b => b.Event.CustomerId == customerId,
                b => b.Event);

        return bookings.Select(MapToDto);
    }

    public async Task<IEnumerable<BookingDto>> GetVendorBookingsAsync(int vendorId)
    {
        var bookings = await _unitOfWork
            .Repository<Booking>()
            .FindWithIncludeAsync(
                b => b.WorkPost.VendorProfileId == vendorId,
                b => b.WorkPost);

        return bookings.Select(MapToDto);
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
            EventId = booking.EventId,
            WorkPostId = booking.WorkPostId,
            BookingDate = booking.BookingDate,
            Status = booking.Status,
            TotalPrice = booking.TotalPrice,
            Quantity = booking.Quantity,
            Notes = booking.Notes
        };
    }
}