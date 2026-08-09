namespace EventHub.Application.DTOs.Admin;

/// <summary>
/// GET /api/admin/reports/analytics
/// Consolidated revenue and usage metrics for the platform reports page.
/// </summary>
public class AdminReportDto
{
    // ── Revenue ────────────────────────────────────────────────────────────────
    public decimal TotalRevenue { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public decimal TotalCommissionEarned { get; set; }
    public decimal CommissionThisMonth { get; set; }

    // ── Bookings ───────────────────────────────────────────────────────────────
    public int TotalBookings { get; set; }
    public int CompletedBookings { get; set; }
    public int CancelledBookings { get; set; }
    public double BookingCompletionRate { get; set; }

    // ── Users ──────────────────────────────────────────────────────────────────
    public int TotalUsers { get; set; }
    public int NewUsersThisMonth { get; set; }
    public int TotalVendors { get; set; }
    public int ActiveVendors { get; set; }

    // ── Monthly Breakdown ──────────────────────────────────────────────────────
    public List<AdminMonthlyRevenueDto> MonthlyRevenue { get; set; } = new();

    // ── Top Vendors ────────────────────────────────────────────────────────────
    public List<TopVendorDto> TopVendors { get; set; } = new();
}

public class AdminMonthlyRevenueDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal GrossRevenue { get; set; }
    public decimal Commission { get; set; }
    public int BookingCount { get; set; }
}

public class TopVendorDto
{
    public int VendorProfileId { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public decimal TotalRevenue { get; set; }
    public int CompletedBookings { get; set; }
    public double AverageRating { get; set; }
}
