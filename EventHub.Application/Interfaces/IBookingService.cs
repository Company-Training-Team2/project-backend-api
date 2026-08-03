using EventHub.Application.DTOs.Booking;

public interface IBookingService
{
    Task<BookingDto> CreateAsync(CreateBookingDto dto);

    Task<BookingDto> AcceptAsync(int bookingId);

    Task<BookingDto> RejectAsync(int bookingId);

    Task<BookingDto> CancelAsync(int bookingId);

    Task<BookingDto> GetByIdAsync(int bookingId);

    Task<IEnumerable<BookingDto>> GetCustomerBookingsAsync(int customerId);
    Task<IEnumerable<BookingDto>> GetVendorBookingsAsync(int vendorId);
}