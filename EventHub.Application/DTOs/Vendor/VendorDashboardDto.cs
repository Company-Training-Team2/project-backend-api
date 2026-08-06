namespace EventHub.Application.DTOs.Vendor;

/// <summary>
/// GET /api/vendor/dashboard
/// Aggregated KPIs shown on the vendor's home screen.
/// </summary>
public class VendorDashboardDto
{
    public int TotalBookings { get; set; }
    public int PendingBookings { get; set; }
    public int ConfirmedBookings { get; set; }
    public int CompletedBookings { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal MonthRevenue { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public int TotalWorkPosts { get; set; }
    public List<UpcomingVendorBookingDto> UpcomingBookings { get; set; } = new();
}