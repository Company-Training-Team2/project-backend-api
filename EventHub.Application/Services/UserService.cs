using EventHub.Application.DTOs;
using EventHub.Application.Interfaces;
using EventHub.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

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

    //private async Task<User> GetCurrentUserInternalAsync()
    //{
    //    var userId = _httpContextAccessor.HttpContext?
    //        .User.FindFirstValue(ClaimTypes.NameIdentifier);

    //    if (userId == null)
    //        throw new UnauthorizedAccessException("User not logged in.");

    //    var user = await _userManager.FindByIdAsync(userId);

    //    if (user == null)
    //        throw new Exception("User not found.");

    //    return user;
    //}

    private async Task<User> GetCurrentUserInternalAsync()
    {
        // TEMPORARY TEST ONLY - remove after JWT authentication is merged

        var user = await _userManager.FindByEmailAsync("test@test.com");

        if (user == null)
            throw new Exception("Test user not found.");

        return user;
    }

    public async Task<UserProfileDto> GetCurrentUserAsync()
    {
        var user = await GetCurrentUserInternalAsync();

        return new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email!,
            Role = user.Role.ToString()
        };
    }

    public async Task<UserProfileDto> UpdateUserAsync(UpdateUserDto dto)
    {
        var user = await GetCurrentUserInternalAsync();

        // Update email
        string? emailToken = null;

        if (!string.IsNullOrWhiteSpace(dto.Email) &&
            dto.Email != user.Email)
        {
            user.Email = dto.Email;
            user.UserName = dto.Email;
            user.EmailConfirmed = false;

            emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        }

        // Update password
        if (!string.IsNullOrWhiteSpace(dto.NewPassword))
        {
            if (string.IsNullOrWhiteSpace(dto.CurrentPassword))
            {
                throw new Exception("Current password is required.");
            }

            var passwordResult = await _userManager.ChangePasswordAsync(
                user,
                dto.CurrentPassword,
                dto.NewPassword);

            if (!passwordResult.Succeeded)
            {
                throw new Exception("Password update failed.");
            }
        }

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            throw new Exception("Failed to update account.");
        }

        return new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email!,
            Role = user.Role.ToString(),
            EmailConfirmationToken = emailToken

        };
    }

    public async Task<bool> DeactivateAccountAsync()
    {
        var user = await GetCurrentUserInternalAsync();

        if (user.IsDeleted)
        {
            throw new InvalidOperationException("Account is already deactivated.");
        }

        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            throw new Exception("Failed to deactivate account.");
        }

        return true;
    }
    public async Task<bool> ConfirmEmailAsync(string token)
    {
        var user = await GetCurrentUserInternalAsync();

        var result = await _userManager.ConfirmEmailAsync(
            user,
            token
        );

        if (!result.Succeeded)
        {
            throw new Exception("Email confirmation failed.");
        }

        return true;
    }

}