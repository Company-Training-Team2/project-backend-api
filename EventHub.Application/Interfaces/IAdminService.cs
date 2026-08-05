using EventHub.Application.DTOs;

namespace EventHub.Application.Interfaces;

public interface IAdminService
{
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

    Task<bool> ApproveVendorAsync(
        int vendorProfileId,
        string? reason);

    Task<bool> RejectVendorAsync(
        int vendorProfileId,
        string? reason);

    Task<bool> RequestVendorChangesAsync(
        int vendorProfileId,
        string? reason);

    // ───────────────── Dashboard ─────────────────

    Task<AdminDashboardDto> GetDashboardAsync();
}