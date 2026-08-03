using EventHub.Application.DTOs.Booking;
using EventHub.Application.Interfaces;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;

namespace EventHub.Application.Services;

public class BookingService : IBookingService
{
    private readonly IUnitOfWork _unitOfWork;

    public BookingService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<BookingDto> CreateAsync(CreateBookingDto dto)
    {
        // Check if Event exists
        var eventEntity = await _unitOfWork
            .Repository<Event>()
            .GetByIdAsync(dto.EventId);

        if (eventEntity is null)
            throw new Exception("Event not found.");

        // Check if WorkPost exists
        var workPost = await _unitOfWork
            .Repository<WorkPost>()
            .GetByIdAsync(dto.WorkPostId);

        if (workPost is null)
            throw new Exception("Work post not found.");

        // Check availability
        var availability = await _unitOfWork
            .Repository<WorkPostAvailability>()
            .FirstOrDefaultAsync(x =>
                x.WorkPostId == dto.WorkPostId &&
                x.Date == dto.BookingDate);

        if (availability is null || !availability.IsAvailable)
            throw new Exception("Selected date is not available.");

        // Calculate total price
        decimal totalPrice = workPost.Price * dto.Quantity;

        // Create Booking
        var booking = new Booking
        {
            EventId = dto.EventId,
            WorkPostId = dto.WorkPostId,
            BookingDate = dto.BookingDate,
            Quantity = dto.Quantity,
            Notes = dto.Notes,
            TotalPrice = totalPrice,
            Status = BookingStatus.Pending
        };

        // Save Booking
        await _unitOfWork
            .Repository<Booking>()
            .AddAsync(booking);

        await _unitOfWork.SaveChangesAsync();

        // Return DTO
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

        // Free the date again
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

        // Make the date available again
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