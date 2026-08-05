using EventHub.Application.DTOs;

namespace EventHub.Application.Interfaces;

public interface IUserService
{
    /// <summary>GET /api/users/me — returns real user data from DB, not mock.</summary>
    Task<UserProfileDto> GetCurrentUserAsync();

    /// <summary>PUT /api/users/me — update profile fields.</summary>
    Task<UserProfileDto> UpdateProfileAsync(UpdateUserDto dto);

    /// <summary>PUT /api/users/me/password — dedicated password change.</summary>
    Task ChangePasswordAsync(ChangePasswordDto dto);

    /// <summary>DELETE /api/users/me/deactivate — soft-delete the account.</summary>
    Task DeactivateAccountAsync();

    /// <summary>GET /api/users/me/activity — audit log of recent account actions.</summary>
    Task<IEnumerable<UserActivityDto>> GetActivityLogAsync(int pageNumber, int pageSize);
}