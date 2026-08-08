using EventHub.Application.DTOs.Admin;
using EventHub.Application.DTOs.Auth;
using EventHub.Application.Interfaces;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Application.Services;

public class AdminService : IAdminService
{
    private readonly UserManager<User> _userManager;
    private readonly IUnitOfWork _unitOfWork;

    public AdminService(UserManager<User> userManager, IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _unitOfWork = unitOfWork;
    }

    // ═══════════════════════════════════════════════════════════
    // Dashboard — GET /api/admin/dashboard
    // ═══════════════════════════════════════════════════════════
    public async Task<AdminDashboardDto> GetDashboardAsync()
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var allUsers = _userManager.Users;
        var totalUsers    = await allUsers.CountAsync();
        var totalCustomers = await allUsers.CountAsync(u => u.Role == UserRole.Customer);
        var totalVendors  = await allUsers.CountAsync(u => u.Role == UserRole.Vendor);

        var pendingVendors = await _unitOfWork.Repository<VendorProfile>()
            .CountAsync(v => v.ApprovalStatus == ApprovalStatus.Pending);

        var bookings = await _unitOfWork.Repository<Booking>().Query().ToListAsync();
        var totalBookings  = bookings.Count;
        var bookingsMonth  = bookings.Count(b => b.CreatedAt >= monthStart);

        var completedBookings = bookings.Where(b => b.Status == BookingStatus.Completed).ToList();
        var totalRevenue   = completedBookings.Sum(b => b.TotalPrice);
        var revenueMonth   = completedBookings
            .Where(b => b.CreatedAt >= monthStart)
            .Sum(b => b.TotalPrice);

        var totalEvents    = await _unitOfWork.Repository<Event>().CountAsync(null);
        var activeWorkPosts = await _unitOfWork.Repository<WorkPost>()
            .CountAsync(w => w.ApprovalStatus == ApprovalStatus.Approved);

        return new AdminDashboardDto
        {
            TotalUsers             = totalUsers,
            TotalCustomers         = totalCustomers,
            TotalVendors           = totalVendors,
            PendingVendorApprovals = pendingVendors,
            TotalBookings          = totalBookings,
            BookingsThisMonth      = bookingsMonth,
            TotalRevenue           = totalRevenue,
            RevenueThisMonth       = revenueMonth,
            TotalEvents            = totalEvents,
            ActiveWorkPosts        = activeWorkPosts
        };
    }

    // ═══════════════════════════════════════════════════════════
    // Users — GET /api/admin/users
    // ═══════════════════════════════════════════════════════════
    public async Task<IEnumerable<AdminUserDto>> GetUsersAsync(
        string? role,
        bool? isDeleted,
        int page,
        int pageSize)
    {
        var query = _userManager.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(role) &&
            Enum.TryParse<UserRole>(role, ignoreCase: true, out var parsedRole))
        {
            query = query.Where(u => u.Role == parsedRole);
        }

        if (isDeleted.HasValue)
            query = query.Where(u => u.IsDeleted == isDeleted.Value);

        // Fetch users with their profile snapshots in a single round-trip
        var users = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var userIds = users.Select(u => u.Id).ToList();

        var customerProfiles = await _unitOfWork.Repository<CustomerProfile>()
            .Query()
            .Where(p => userIds.Contains(p.UserId))
            .ToListAsync();

        var vendorProfiles = await _unitOfWork.Repository<VendorProfile>()
            .Query()
            .Where(p => userIds.Contains(p.UserId))
            .ToListAsync();

        return users.Select(u => new AdminUserDto
        {
            Id              = u.Id,
            Email           = u.Email!,
            Role            = u.Role.ToString(),
            IsActive        = u.IsActive,
            IsDeleted       = u.IsDeleted,
            DeletedAt       = u.DeletedAt,
            IsEmailVerified = u.IsEmailVerified,
            CreatedAt       = u.CreatedAt,
            FullName        = customerProfiles.FirstOrDefault(p => p.UserId == u.Id)?.FullName,
            BusinessName    = vendorProfiles.FirstOrDefault(p => p.UserId == u.Id)?.BusinessName
        });
    }

    // ── PUT /api/admin/users/{id}/suspend ─────────────────────────────────────
    public async Task<bool> SuspendUserAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return false;

        user.IsActive  = false;
        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;

        return (await _userManager.UpdateAsync(user)).Succeeded;
    }

    // ── PUT /api/admin/users/{id}/activate ────────────────────────────────────
    public async Task<bool> ActivateUserAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null) return false;

        user.IsActive  = true;
        user.IsDeleted = false;
        user.DeletedAt = null;

        return (await _userManager.UpdateAsync(user)).Succeeded;
    }

    // ═══════════════════════════════════════════════════════════
    // Vendors — GET /api/admin/vendors/pending
    // ═══════════════════════════════════════════════════════════
    public async Task<IEnumerable<AdminVendorDto>> GetPendingVendorsAsync()
    {
        var pending = await _unitOfWork.Repository<VendorProfile>()
            .Query()
            .Include(v => v.User)
            .Where(v => v.ApprovalStatus == ApprovalStatus.Pending)
            .OrderBy(v => v.User.CreatedAt)
            .ToListAsync();

        return pending.Select(MapToAdminVendorDto);
    }

    // ── GET /api/admin/vendors ────────────────────────────────────────────────
    public async Task<IEnumerable<AdminVendorDto>> GetAllVendorsAsync(
        string? approvalStatus,
        int page,
        int pageSize)
    {
        var query = _unitOfWork.Repository<VendorProfile>()
            .Query()
            .Include(v => v.User)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(approvalStatus) &&
            Enum.TryParse<ApprovalStatus>(approvalStatus, ignoreCase: true, out var parsedStatus))
        {
            query = query.Where(v => v.ApprovalStatus == parsedStatus);
        }

        var vendors = await query
            .OrderByDescending(v => v.User.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return vendors.Select(MapToAdminVendorDto);
    }

    // ── PUT /api/admin/vendors/{id}/approve ───────────────────────────────────
    public async Task<bool> ApproveVendorAsync(int vendorProfileId, string? reason)
    {
        var vendor = await _unitOfWork.Repository<VendorProfile>()
            .GetByIdAsync(vendorProfileId);

        if (vendor is null) return false;

        vendor.ApprovalStatus = ApprovalStatus.Approved;
        vendor.IsVerified     = true;
        _unitOfWork.Repository<VendorProfile>().Update(vendor);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    // ── PUT /api/admin/vendors/{id}/reject ────────────────────────────────────
    public async Task<bool> RejectVendorAsync(int vendorProfileId, string? reason)
    {
        var vendor = await _unitOfWork.Repository<VendorProfile>()
            .GetByIdAsync(vendorProfileId);

        if (vendor is null) return false;

        vendor.ApprovalStatus = ApprovalStatus.Rejected;
        _unitOfWork.Repository<VendorProfile>().Update(vendor);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    // ── PUT /api/admin/vendors/{id}/request-changes ───────────────────────────
    public async Task<bool> RequestVendorChangesAsync(int vendorProfileId, string? reason)
    {
        // Reset back to Pending so the vendor can re-submit; reason is
        // communicated via the CRM or a notification (wired separately).
        var vendor = await _unitOfWork.Repository<VendorProfile>()
            .GetByIdAsync(vendorProfileId);

        if (vendor is null) return false;

        vendor.ApprovalStatus = ApprovalStatus.Pending;
        _unitOfWork.Repository<VendorProfile>().Update(vendor);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    // ═══════════════════════════════════════════════════════════
    // Reports — GET /api/admin/reports/analytics
    // ═══════════════════════════════════════════════════════════
    public async Task<AdminReportDto> GetAnalyticsReportAsync()
    {
        var now        = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        // ── Bookings ──────────────────────────────────────────────────────────
        var bookings = await _unitOfWork.Repository<Booking>()
            .Query()
            .Include(b => b.WorkPost)
                .ThenInclude(w => w.VendorProfile)
            .Include(b => b.WorkPost)
                .ThenInclude(w => w.Bookings)
                    .ThenInclude(wb => wb.Review)
            .ToListAsync();

        var completed  = bookings.Where(b => b.Status == BookingStatus.Completed).ToList();
        var cancelled  = bookings.Where(b => b.Status == BookingStatus.Rejected).ToList();

        // ── Settings (commission rate) ─────────────────────────────────────────
        var settings = await GetOrCreateSettingsAsync();
        var commissionRate = settings.CommissionPercentage / 100m;

        var totalRevenue     = completed.Sum(b => b.TotalPrice);
        var revenueMonth     = completed.Where(b => b.CreatedAt >= monthStart).Sum(b => b.TotalPrice);
        var totalCommission  = totalRevenue * commissionRate;
        var commissionMonth  = revenueMonth * commissionRate;

        // ── Users ─────────────────────────────────────────────────────────────
        var totalUsers    = await _userManager.Users.CountAsync();
        var newUsersMonth = await _userManager.Users.CountAsync(u => u.CreatedAt >= monthStart);
        var totalVendors  = await _userManager.Users.CountAsync(u => u.Role == UserRole.Vendor);
        var activeVendors = await _unitOfWork.Repository<VendorProfile>()
            .CountAsync(v => v.ApprovalStatus == ApprovalStatus.Approved);

        // ── Monthly breakdown ─────────────────────────────────────────────────
        var monthly = completed
            .GroupBy(b => new { b.CreatedAt.Year, b.CreatedAt.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g =>
            {
                var gross = g.Sum(b => b.TotalPrice);
                return new AdminMonthlyRevenueDto
                {
                    Year         = g.Key.Year,
                    Month        = g.Key.Month,
                    GrossRevenue = gross,
                    Commission   = gross * commissionRate,
                    BookingCount = g.Count()
                };
            })
            .ToList();

        // ── Top vendors ───────────────────────────────────────────────────────
        var topVendors = completed
            .GroupBy(b => b.WorkPost.VendorProfileId)
            .Select(g =>
            {
                var vendorProfile = g.First().WorkPost.VendorProfile;
                var allVendorBookings = bookings
                    .Where(b => b.WorkPost.VendorProfileId == g.Key)
                    .ToList();
                var reviews = allVendorBookings
                    .SelectMany(b => b.WorkPost.Bookings)
                    .Where(b => b.Review != null)
                    .Select(b => b.Review!)
                    .ToList();

                return new TopVendorDto
                {
                    VendorProfileId   = g.Key,
                    BusinessName      = vendorProfile.BusinessName,
                    TotalRevenue      = g.Sum(b => b.TotalPrice),
                    CompletedBookings = g.Count(),
                    AverageRating     = reviews.Count > 0
                        ? Math.Round(reviews.Average(r => (double)r.Rating), 2)
                        : 0
                };
            })
            .OrderByDescending(v => v.TotalRevenue)
            .Take(10)
            .ToList();

        return new AdminReportDto
        {
            TotalRevenue          = totalRevenue,
            RevenueThisMonth      = revenueMonth,
            TotalCommissionEarned = totalCommission,
            CommissionThisMonth   = commissionMonth,
            TotalBookings         = bookings.Count,
            CompletedBookings     = completed.Count,
            CancelledBookings     = cancelled.Count,
            BookingCompletionRate = bookings.Count > 0
                ? Math.Round((double)completed.Count / bookings.Count * 100, 2)
                : 0,
            TotalUsers        = totalUsers,
            NewUsersThisMonth = newUsersMonth,
            TotalVendors      = totalVendors,
            ActiveVendors     = activeVendors,
            MonthlyRevenue    = monthly,
            TopVendors        = topVendors
        };
    }

    // ═══════════════════════════════════════════════════════════
    // Settings — GET /PUT /api/admin/settings
    // ═══════════════════════════════════════════════════════════
    public async Task<AdminSettingsDto> GetSettingsAsync()
    {
        var settings = await GetOrCreateSettingsAsync();
        return MapToSettingsDto(settings);
    }

    public async Task<AdminSettingsDto> UpdateSettingsAsync(UpdateAdminSettingsDto dto)
    {
        var settings = await GetOrCreateSettingsAsync();

        if (dto.CommissionPercentage.HasValue)
            settings.CommissionPercentage = dto.CommissionPercentage.Value;
        if (dto.TaxPercentage.HasValue)
            settings.TaxPercentage = dto.TaxPercentage.Value;
        if (dto.MaxImagesPerWorkPost.HasValue)
            settings.MaxImagesPerWorkPost = dto.MaxImagesPerWorkPost.Value;
        if (dto.MaxPackagesPerWorkPost.HasValue)
            settings.MaxPackagesPerWorkPost = dto.MaxPackagesPerWorkPost.Value;
        if (dto.PlatformName is not null)
            settings.PlatformName = dto.PlatformName;
        if (dto.PlatformLogoUrl is not null)
            settings.PlatformLogoUrl = dto.PlatformLogoUrl;
        if (dto.SupportEmail is not null)
            settings.SupportEmail = dto.SupportEmail;

        settings.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Repository<AdminSettings>().Update(settings);
        await _unitOfWork.SaveChangesAsync();

        return MapToSettingsDto(settings);
    }

    // ═══════════════════════════════════════════════════════════
    // CRM Conversations — GET /POST /api/admin/conversations
    // ═══════════════════════════════════════════════════════════
    public async Task<IEnumerable<AdminConversationDto>> GetConversationsAsync()
    {
        var conversations = await _unitOfWork.Repository<AdminConversation>()
            .Query()
            .Include(c => c.User)
            .Include(c => c.Messages)
            .OrderByDescending(c => c.UpdatedAt)
            .ToListAsync();

        return conversations.Select(c =>
        {
            var lastMsg = c.Messages
                .OrderByDescending(m => m.SentAt)
                .FirstOrDefault();

            var unread = c.Messages.Count(m => !m.IsReadByAdmin);

            return new AdminConversationDto
            {
                Id                  = c.Id,
                UserId              = c.UserId,
                UserEmail           = c.User.Email!,
                UserDisplayName     = null, // enriched by CustomerProfile if needed
                Subject             = c.Subject,
                Status              = c.Status,
                LastMessageSnippet  = lastMsg?.Body.Length > 80
                    ? lastMsg.Body[..80] + "…"
                    : lastMsg?.Body,
                CreatedAt           = c.CreatedAt,
                UpdatedAt           = c.UpdatedAt,
                UnreadCount         = unread
            };
        });
    }

    public async Task<AdminConversationDto> CreateConversationAsync(CreateAdminConversationDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.UserId.ToString())
            ?? throw new InvalidOperationException($"User {dto.UserId} not found.");

        var conversation = new AdminConversation
        {
            UserId    = dto.UserId,
            Subject   = dto.Subject,
            Status    = "Open",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        if (!string.IsNullOrWhiteSpace(dto.InitialMessage))
        {
            conversation.Messages.Add(new AdminConversationMessage
            {
                Body          = dto.InitialMessage,
                SenderUserId  = null,    // null = sent by admin
                IsReadByUser  = false,
                IsReadByAdmin = true,
                SentAt        = DateTime.UtcNow
            });
        }

        await _unitOfWork.Repository<AdminConversation>().AddAsync(conversation);
        await _unitOfWork.SaveChangesAsync();

        return new AdminConversationDto
        {
            Id                 = conversation.Id,
            UserId             = conversation.UserId,
            UserEmail          = user.Email!,
            Subject            = conversation.Subject,
            Status             = conversation.Status,
            LastMessageSnippet = dto.InitialMessage?.Length > 80
                ? dto.InitialMessage[..80] + "…"
                : dto.InitialMessage,
            CreatedAt          = conversation.CreatedAt,
            UpdatedAt          = conversation.UpdatedAt,
            UnreadCount        = 0
        };
    }

    // ═══════════════════════════════════════════════════════════
    // Private helpers
    // ═══════════════════════════════════════════════════════════

    /// <summary>
    /// Returns the singleton AdminSettings row (Id = 1), seeding it with
    /// safe defaults if it doesn't exist yet.
    /// </summary>
    private async Task<AdminSettings> GetOrCreateSettingsAsync()
    {
        var existing = await _unitOfWork.Repository<AdminSettings>()
            .FirstOrDefaultAsync(s => s.Id == 1);

        if (existing is not null) return existing;

        var defaults = new AdminSettings { Id = 1 };
        await _unitOfWork.Repository<AdminSettings>().AddAsync(defaults);
        await _unitOfWork.SaveChangesAsync();
        return defaults;
    }

    private static AdminSettingsDto MapToSettingsDto(AdminSettings s) => new()
    {
        CommissionPercentage  = s.CommissionPercentage,
        TaxPercentage         = s.TaxPercentage,
        MaxImagesPerWorkPost  = s.MaxImagesPerWorkPost,
        MaxPackagesPerWorkPost = s.MaxPackagesPerWorkPost,
        PlatformName          = s.PlatformName,
        PlatformLogoUrl       = s.PlatformLogoUrl,
        SupportEmail          = s.SupportEmail,
        UpdatedAt             = s.UpdatedAt
    };

    private static AdminVendorDto MapToAdminVendorDto(VendorProfile v) => new()
    {
        VendorProfileId = v.Id,
        UserId          = v.UserId,
        Email           = v.User.Email!,
        BusinessName    = v.BusinessName,
        BioDescription  = v.BioDescription,
        PhoneNumber     = v.PhoneNumber,
        City            = v.City,
        ApprovalStatus  = v.ApprovalStatus.ToString(),
        IsVerified      = v.IsVerified,
        IsDeleted       = v.IsDeleted,
        CreatedAt       = v.User.CreatedAt
    };
}
