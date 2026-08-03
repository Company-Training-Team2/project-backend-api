using EventHub.Application.DTOs;

namespace EventHub.Application.Interfaces;

public interface IAdminUserService
{
    Task<List<AdminUserDto>> GetUsersAsync(
        string? role,
        bool? isDeleted);

    Task<bool> SuspendUserAsync(int userId);

    Task<bool> ActivateUserAsync(int userId);
}