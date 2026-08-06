namespace EventHub.Application.DTOs;

/// <summary>
/// One linked vendor booking for an event, per audit Module 3/4
/// ("Linked vendors grouped by booking status: booked / pending").
/// Group by BookingStatus client-side, or filter server-side by status.
/// </summary>
public class EventVendorResponse
{
    public int BookingId { get; set; }
    public int VendorProfileId { get; set; }
    public string VendorName { get; set; } = string.Empty;
    public string ServiceTitle { get; set; } = string.Empty;
    public string BookingStatus { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime BookingDate { get; set; }
}
