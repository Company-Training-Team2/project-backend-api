namespace EventHub.Application.DTOs.Vendor;

public class UpcomingVendorBookingDto
{
    public int BookingId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string WorkPostTitle { get; set; } = string.Empty;
    public DateOnly BookingDate { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = string.Empty;
}