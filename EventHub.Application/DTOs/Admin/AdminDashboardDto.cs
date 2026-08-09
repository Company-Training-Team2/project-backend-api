namespace EventHub.Application.DTOs.Admin;

/// <summary>Aggregated KPIs returned by GET /api/admin/dashboard.</summary>
public class AdminDashboardDto
{
    public int TotalUsers { get; set; }
    public int TotalCustomers { get; set; }
    public int TotalVendors { get; set; }
    public int PendingVendorApprovals { get; set; }
    public int TotalBookings { get; set; }
    public int BookingsThisMonth { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public int TotalEvents { get; set; }
    public int ActiveWorkPosts { get; set; }
}