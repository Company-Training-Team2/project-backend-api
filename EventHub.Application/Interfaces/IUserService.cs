using EventHub.Application.DTOs;

namespace EventHub.Application.Interfaces;

public interface IUserService
{
    Task<UserProfileDto> GetCurrentUserAsync();

    Task<UserProfileDto> UpdateUserAsync(UpdateUserDto dto);

    Task DeactivateAccountAsync();

}
