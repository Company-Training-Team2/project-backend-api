using EventHub.Application.DTOs.Booking;

namespace EventHub.Application.Interfaces;

public interface IBookingService
{
    Task<BookingDto> CreateAsync(CreateBookingDto dto);
}