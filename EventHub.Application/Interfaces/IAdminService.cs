using EventHub.Application.DTOs.Admin;
using EventHub.Application.DTOs.Auth;

namespace EventHub.Application.Interfaces;

public interface IAdminService
{
    // ───────────────── Dashboard ─────────────────
    Task<AdminDashboardDto> GetDashboardAsync();

    // ───────────────── Users ─────────────────
    Task<IEnumerable<AdminUserDto>> GetUsersAsync(
        string? role,
        bool? isDeleted,
        int page,
        int pageSize);

    Task<bool> SuspendUserAsync(int userId);
    Task<bool> ActivateUserAsync(int userId);

    // ───────────────── Vendors ─────────────────
    Task<IEnumerable<AdminVendorDto>> GetPendingVendorsAsync();

    Task<IEnumerable<AdminVendorDto>> GetAllVendorsAsync(
        string? approvalStatus,
        int page,
        int pageSize);

    Task<bool> ApproveVendorAsync(int vendorProfileId, string? reason);
    Task<bool> RejectVendorAsync(int vendorProfileId, string? reason);
    Task<bool> RequestVendorChangesAsync(int vendorProfileId, string? reason);

    // ───────────────── Reports / Analytics ─────────────────
    Task<AdminReportDto> GetAnalyticsReportAsync();

    // ───────────────── Settings ─────────────────
    Task<AdminSettingsDto> GetSettingsAsync();
    Task<AdminSettingsDto> UpdateSettingsAsync(UpdateAdminSettingsDto dto);

    // ───────────────── CRM Conversations ─────────────────
    Task<IEnumerable<AdminConversationDto>> GetConversationsAsync();
    Task<AdminConversationDto> CreateConversationAsync(CreateAdminConversationDto dto);
}
