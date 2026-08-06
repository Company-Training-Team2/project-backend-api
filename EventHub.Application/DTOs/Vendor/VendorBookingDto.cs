namespace EventHub.Application.DTOs.Vendor;

/// <summary>
/// Vendor-side view of a booking — richer than the customer BookingDto.
/// </summary>
public class VendorBookingDto
{
    public int Id { get; set; }
    public int WorkPostId { get; set; }
    public string WorkPostTitle { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public DateOnly BookingDate { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}