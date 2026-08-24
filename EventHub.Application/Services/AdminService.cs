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

        // Same fix as VendorService.GetAnalyticsAsync: revenue is money
        // actually received (Status == Paid, set the moment
        // PaymentService.ProcessAsync succeeds), not just bookings a vendor
        // has separately, manually marked Completed after the fact — the
        // admin dashboard's headline revenue figure was understating real
        // platform revenue by however much sat in Paid awaiting that.
        var revenueBookings = bookings
            .Where(b => b.Status is BookingStatus.Paid or BookingStatus.Completed)
            .ToList();
        var totalRevenue   = revenueBookings.Sum(b => b.TotalPrice);
        var revenueMonth   = revenueBookings
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
    // Vendor Service Listings — GET /api/admin/workposts/pending
    // ═══════════════════════════════════════════════════════════
    public async Task<IEnumerable<AdminWorkPostDto>> GetPendingWorkPostsAsync()
    {
        var pending = await _unitOfWork.Repository<WorkPost>()
            .Query()
            .Include(w => w.VendorProfile)
            .Include(w => w.Category)
            .Include(w => w.Images)
            .Where(w => w.ApprovalStatus == ApprovalStatus.Pending)
            .OrderBy(w => w.CreatedAt)
            .ToListAsync();

        return pending.Select(MapToAdminWorkPostDto);
    }

    // ── GET /api/admin/workposts ──────────────────────────────────────────────
    public async Task<IEnumerable<AdminWorkPostDto>> GetAllWorkPostsAsync(
        string? approvalStatus,
        int page,
        int pageSize)
    {
        var query = _unitOfWork.Repository<WorkPost>()
            .Query()
            .Include(w => w.VendorProfile)
            .Include(w => w.Category)
            .Include(w => w.Images)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(approvalStatus) &&
            Enum.TryParse<ApprovalStatus>(approvalStatus, ignoreCase: true, out var parsedStatus))
        {
            query = query.Where(w => w.ApprovalStatus == parsedStatus);
        }

        var workPosts = await query
            .OrderByDescending(w => w.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return workPosts.Select(MapToAdminWorkPostDto);
    }

    // ── PUT /api/admin/workposts/{id}/approve ─────────────────────────────────
    public async Task<bool> ApproveWorkPostAsync(int workPostId, int adminUserId)
    {
        var workPost = await _unitOfWork.Repository<WorkPost>()
            .GetByIdAsync(workPostId);

        if (workPost is null) return false;

        workPost.ApprovalStatus    = ApprovalStatus.Approved;
        workPost.ReviewedByAdminId = adminUserId;
        _unitOfWork.Repository<WorkPost>().Update(workPost);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    // ── PUT /api/admin/workposts/{id}/reject ──────────────────────────────────
    public async Task<bool> RejectWorkPostAsync(int workPostId, int adminUserId, string? reason)
    {
        // Same as RejectVendorAsync: reason isn't persisted anywhere on
        // WorkPost — it's accepted here for parity with the vendor-decision
        // endpoints and communicated to the vendor via CRM/notification,
        // wired separately.
        var workPost = await _unitOfWork.Repository<WorkPost>()
            .GetByIdAsync(workPostId);

        if (workPost is null) return false;

        workPost.ApprovalStatus    = ApprovalStatus.Rejected;
        workPost.ReviewedByAdminId = adminUserId;
        _unitOfWork.Repository<WorkPost>().Update(workPost);
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
        // Same fix as GetDashboardAsync/VendorService.GetAnalyticsAsync:
        // revenue/commission is money actually received (Paid), not just
        // the subset a vendor has separately, manually marked Completed.
        var revenueBookings = bookings
            .Where(b => b.Status is BookingStatus.Paid or BookingStatus.Completed)
            .ToList();

        // ── Settings (commission rate) ─────────────────────────────────────────
        var settings = await GetOrCreateSettingsAsync();
        var commissionRate = settings.CommissionPercentage / 100m;

        var totalRevenue     = revenueBookings.Sum(b => b.TotalPrice);
        var revenueMonth     = revenueBookings.Where(b => b.CreatedAt >= monthStart).Sum(b => b.TotalPrice);
        var totalCommission  = totalRevenue * commissionRate;
        var commissionMonth  = revenueMonth * commissionRate;

        // ── Users ─────────────────────────────────────────────────────────────
        var totalUsers    = await _userManager.Users.CountAsync();
        var newUsersMonth = await _userManager.Users.CountAsync(u => u.CreatedAt >= monthStart);
        var totalVendors  = await _userManager.Users.CountAsync(u => u.Role == UserRole.Vendor);
        var activeVendors = await _unitOfWork.Repository<VendorProfile>()
            .CountAsync(v => v.ApprovalStatus == ApprovalStatus.Approved);

        // ── Monthly breakdown ─────────────────────────────────────────────────
        var monthly = revenueBookings
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
        // WorkPost carries the generic soft-delete query filter (see
        // ApplicationDbContext), and Booking.WorkPost is a required
        // (non-nullable) navigation — so a completed Booking whose WorkPost
        // listing was later soft-deleted comes back from the .Include()
        // above with WorkPost == null at runtime despite the `null!`
        // annotation, throwing here on `.VendorProfile` (same for a
        // soft-deleted VendorProfile). This 500'd the whole analytics
        // report for every admin the moment any one listing/vendor was
        // deleted. Total revenue/booking counts above don't touch WorkPost
        // so they're unaffected; only the per-vendor ranking, which can't
        // attribute revenue to a listing/vendor that no longer resolves,
        // needs to skip those bookings.
        var topVendors = revenueBookings
            .Where(b => b.WorkPost?.VendorProfile != null)
            .GroupBy(b => b.WorkPost.VendorProfileId)
            .Select(g =>
            {
                var vendorProfile = g.First().WorkPost.VendorProfile;
                var allVendorBookings = bookings
                    .Where(b => b.WorkPost?.VendorProfileId == g.Key)
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
                    // Grouped from revenueBookings (Paid + Completed) now,
                    // so this can't just be g.Count() any more — narrow back
                    // down to actually-Completed within this vendor's group
                    // to keep the field meaning what its name says.
                    CompletedBookings = g.Count(b => b.Status == BookingStatus.Completed),
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

    public async Task<IEnumerable<AdminConversationMessageDto>> GetConversationMessagesAsync(int conversationId)
    {
        var conversation = await _unitOfWork.Repository<AdminConversation>()
            .Query()
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == conversationId)
            ?? throw new InvalidOperationException($"Conversation {conversationId} not found.");

        // Opening this thread from the admin side means the admin has now
        // seen every message the user sent — mirrors NotificationsService's
        // mark-as-read-on-open pattern rather than requiring a separate call.
        var unreadFromUser = conversation.Messages.Where(m => m.SenderUserId != null && !m.IsReadByAdmin).ToList();
        if (unreadFromUser.Count > 0)
        {
            foreach (var m in unreadFromUser) m.IsReadByAdmin = true;
            await _unitOfWork.SaveChangesAsync();
        }

        return conversation.Messages
            .OrderBy(m => m.SentAt)
            .Select(m => new AdminConversationMessageDto
            {
                Id            = m.Id,
                ConversationId = m.ConversationId,
                SenderUserId  = m.SenderUserId,
                Body          = m.Body,
                SentAt        = m.SentAt,
                IsReadByUser  = m.IsReadByUser,
                IsReadByAdmin = m.IsReadByAdmin
            });
    }

    public async Task<AdminConversationMessageDto> SendConversationMessageAsync(int conversationId, SendAdminConversationMessageDto dto)
    {
        var conversation = await _unitOfWork.Repository<AdminConversation>().GetByIdAsync(conversationId)
            ?? throw new InvalidOperationException($"Conversation {conversationId} not found.");

        var message = new AdminConversationMessage
        {
            ConversationId = conversationId,
            SenderUserId   = null, // null = sent by admin (see AdminConversationMessage.SenderUserId)
            Body           = dto.Body,
            IsReadByUser   = false,
            IsReadByAdmin  = true,
            SentAt         = DateTime.UtcNow
        };

        await _unitOfWork.Repository<AdminConversationMessage>().AddAsync(message);

        conversation.UpdatedAt = DateTime.UtcNow;
        if (conversation.Status == "Closed") conversation.Status = "Open";
        _unitOfWork.Repository<AdminConversation>().Update(conversation);

        await _unitOfWork.SaveChangesAsync();

        return new AdminConversationMessageDto
        {
            Id             = message.Id,
            ConversationId = message.ConversationId,
            SenderUserId   = message.SenderUserId,
            Body           = message.Body,
            SentAt         = message.SentAt,
            IsReadByUser   = message.IsReadByUser,
            IsReadByAdmin  = message.IsReadByAdmin
        };
    }

    public async Task<AdminConversationDto> UpdateConversationStatusAsync(int conversationId, string status)
    {
        if (status is not ("Open" or "Resolved" or "Closed"))
            throw new InvalidOperationException("Status must be Open, Resolved, or Closed.");

        var conversation = await _unitOfWork.Repository<AdminConversation>()
            .Query()
            .Include(c => c.User)
            .Include(c => c.Messages)
            .FirstOrDefaultAsync(c => c.Id == conversationId)
            ?? throw new InvalidOperationException($"Conversation {conversationId} not found.");

        conversation.Status = status;
        conversation.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Repository<AdminConversation>().Update(conversation);
        await _unitOfWork.SaveChangesAsync();

        var lastMsg = conversation.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault();
        return new AdminConversationDto
        {
            Id                 = conversation.Id,
            UserId             = conversation.UserId,
            UserEmail          = conversation.User.Email!,
            Subject            = conversation.Subject,
            Status             = conversation.Status,
            LastMessageSnippet = lastMsg?.Body.Length > 80 ? lastMsg.Body[..80] + "…" : lastMsg?.Body,
            CreatedAt          = conversation.CreatedAt,
            UpdatedAt          = conversation.UpdatedAt,
            UnreadCount        = conversation.Messages.Count(m => m.SenderUserId != null && !m.IsReadByAdmin)
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

    private static AdminWorkPostDto MapToAdminWorkPostDto(WorkPost w) => new()
    {
        Id                 = w.Id,
        VendorProfileId    = w.VendorProfileId,
        VendorBusinessName = w.VendorProfile.BusinessName,
        CategoryId         = w.CategoryId,
        CategoryName       = w.Category.Name,
        Title              = w.Title,
        Description        = w.Description,
        Price              = w.Price,
        City               = w.City,
        Address            = w.Address,
        MinGuests          = w.MinGuests,
        MaxGuests          = w.MaxGuests,
        ApprovalStatus     = w.ApprovalStatus.ToString(),
        PrimaryImageUrl    = w.Images
            .OrderByDescending(i => i.IsPrimary)
            .Select(i => i.ImageUrl)
            .FirstOrDefault(),
        ImageUrls          = w.Images
            .OrderByDescending(i => i.IsPrimary)
            .Select(i => i.ImageUrl)
            .ToList(),
        CreatedAt          = w.CreatedAt
    };
}
