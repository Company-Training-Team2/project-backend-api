using EventHub.Application.DTOs.Booking;
using EventHub.Domain.Enums;

public interface IBookingService
{
    Task<BookingDto> CreateAsync(CreateBookingDto dto);

    Task<BookingDto> AcceptAsync(int bookingId);

    Task<BookingDto> RejectAsync(int bookingId);

    Task<BookingDto> CancelAsync(int bookingId);

    Task<BookingDto> GetByIdAsync(int bookingId);

    /// <summary>GET /api/bookings?customerId=&status= — optional status filter per audit Module 8 API contract.</summary>
    Task<IEnumerable<BookingDto>> GetCustomerBookingsAsync(int customerId, BookingStatus? status = null);

    /// <summary>Optional status filter, mirroring the customer-facing list.</summary>
    Task<IEnumerable<BookingDto>> GetVendorBookingsAsync(int vendorId, BookingStatus? status = null);
}