using EventHub.Application.DTOs.Booking;

namespace EventHub.Application.Interfaces;

public interface IBookingService
{
    Task<BookingDto> CreateAsync(CreateBookingDto dto);

    Task<BookingDto> AcceptAsync(int bookingId);

    Task<BookingDto> RejectAsync(int bookingId);

    Task<BookingDto> CancelAsync(int bookingId);
}