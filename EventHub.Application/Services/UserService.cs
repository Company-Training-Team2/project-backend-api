using EventHub.Application.DTOs;
using EventHub.Application.Interfaces;
using EventHub.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
namespace EventHub.Application.Services;

public class UserService : IUserService
{
    private readonly UserManager<User> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserService(
        UserManager<User> userManager,
        IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
    }


    public async Task<UserProfileDto> GetCurrentUserAsync()
    {
        var userId = _httpContextAccessor.HttpContext?
            .User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
            throw new UnauthorizedAccessException("User not logged in");


        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            throw new Exception("User not found");


        return new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email!,
            Role = user.Role.ToString()
        };
    }


    public async Task<UserProfileDto> UpdateUserAsync(UpdateUserDto dto)
    {
        var userId = _httpContextAccessor.HttpContext?
            .User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
            throw new UnauthorizedAccessException("User not logged in");


        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            throw new Exception("User not found");


        // Update email
        if (!string.IsNullOrEmpty(dto.Email) &&
            dto.Email != user.Email)
        {
            user.Email = dto.Email;
            user.UserName = dto.Email;
            user.EmailConfirmed = false;
        }


        // Update password
        if (!string.IsNullOrEmpty(dto.NewPassword))
        {
            if (string.IsNullOrEmpty(dto.CurrentPassword))
            {
                throw new Exception("Current password is required");
            }

            var passwordResult =
                await _userManager.ChangePasswordAsync(
                    user,
                    dto.CurrentPassword,
                    dto.NewPassword);

            if (!passwordResult.Succeeded)
            {
                throw new Exception(
                    "Password update failed");
            }
        }


        await _userManager.UpdateAsync(user);


        return new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email!,
            Role = user.Role.ToString()
        };
    }
}