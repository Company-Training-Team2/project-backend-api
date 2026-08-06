namespace EventHub.Application.DTOs.Vendor;

/// <summary>
/// GET /api/vendor/analytics
/// Revenue, conversion, and view analytics for the vendor portal.
/// </summary>
public class VendorAnalyticsDto
{
    public decimal TotalRevenue { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public int TotalBookings { get; set; }
    public int CompletedBookings { get; set; }
    public double ConversionRate { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public List<MonthlyRevenueDto> MonthlyRevenue { get; set; } = new();
    public List<WorkPostPerformanceDto> WorkPostPerformance { get; set; } = new();
}

public class MonthlyRevenueDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Revenue { get; set; }
    public int BookingCount { get; set; }
}

public class WorkPostPerformanceDto
{
    public int WorkPostId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int TotalBookings { get; set; }
    public decimal TotalRevenue { get; set; }
    public double AverageRating { get; set; }
}