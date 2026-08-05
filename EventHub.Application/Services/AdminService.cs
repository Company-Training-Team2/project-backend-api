using EventHub.Application.DTOs;
using EventHub.Application.Interfaces;
using EventHub.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace EventHub.Application.Services;

public class AdminService : IAdminService
{
    private readonly UserManager<User> _userManager;

    public AdminService(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    // ========================= USERS =========================

    public Task<IEnumerable<AdminUserDto>> GetUsersAsync(
        string? role,
        bool? isDeleted,
        int page,
        int pageSize)
    {
        var users = _userManager.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(role))
        {
            users = users.Where(u => u.Role.ToString() == role);
        }

        if (isDeleted.HasValue)
        {
            users = users.Where(u => u.IsDeleted == isDeleted.Value);
        }

        var result = users
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new AdminUserDto
            {
                Id = u.Id,
                Email = u.Email!,
                Role = u.Role.ToString(),

                IsDeleted = u.IsDeleted,
                DeletedAt = u.DeletedAt,

                IsActive = u.IsActive,
                IsEmailVerified = u.IsEmailVerified,

                CreatedAt = u.CreatedAt
            })
            .ToList();

        return Task.FromResult<IEnumerable<AdminUserDto>>(result);
    }

    public async Task<bool> SuspendUserAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user == null)
            return false;

        user.IsDeleted = true;
        user.IsActive = false;
        user.DeletedAt = DateTime.UtcNow;

        return (await _userManager.UpdateAsync(user)).Succeeded;
    }

    public async Task<bool> ActivateUserAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user == null)
            return false;

        user.IsDeleted = false;
        user.IsActive = true;
        user.DeletedAt = null;

        return (await _userManager.UpdateAsync(user)).Succeeded;
    }

    // ========================= VENDORS =========================

    public Task<IEnumerable<AdminVendorDto>> GetPendingVendorsAsync()
    {
        return Task.FromResult<IEnumerable<AdminVendorDto>>(
            new List<AdminVendorDto>());
    }

    public Task<IEnumerable<AdminVendorDto>> GetAllVendorsAsync(
        string? approvalStatus,
        int page,
        int pageSize)
    {
        return Task.FromResult<IEnumerable<AdminVendorDto>>(
            new List<AdminVendorDto>());
    }

    public Task<bool> ApproveVendorAsync(
        int vendorProfileId,
        string? reason)
    {
        return Task.FromResult(true);
    }

    public Task<bool> RejectVendorAsync(
        int vendorProfileId,
        string? reason)
    {
        return Task.FromResult(true);
    }

    public Task<bool> RequestVendorChangesAsync(
        int vendorProfileId,
        string? reason)
    {
        return Task.FromResult(true);
    }

    // ========================= DASHBOARD =========================

    public Task<AdminDashboardDto> GetDashboardAsync()
    {
        return Task.FromResult(new AdminDashboardDto());
    }
}