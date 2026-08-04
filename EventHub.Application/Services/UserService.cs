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

    private async Task<User> GetCurrentUserInternalAsync()
    {
        var userId = _httpContextAccessor.HttpContext?
            .User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            throw new UnauthorizedAccessException("User is not authenticated.");

        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
            throw new Exception("User not found.");

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

        string? emailToken = null;

        // Update email
        if (!string.IsNullOrWhiteSpace(dto.Email) &&
            dto.Email != user.Email)
        {
            user.Email = dto.Email;
            user.UserName = dto.Email;
            user.EmailConfirmed = false;

            emailToken =
                await _userManager.GenerateEmailConfirmationTokenAsync(user);
        }

        // Update password
        if (!string.IsNullOrWhiteSpace(dto.NewPassword))
        {
            if (string.IsNullOrWhiteSpace(dto.CurrentPassword))
                throw new Exception("Current password is required.");

            var passwordResult = await _userManager.ChangePasswordAsync(
                user,
                dto.CurrentPassword,
                dto.NewPassword);

            if (!passwordResult.Succeeded)
            {
                throw new Exception(
                    string.Join(", ",
                        passwordResult.Errors.Select(e => e.Description)));
            }
        }

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            throw new Exception(
                string.Join(", ",
                    result.Errors.Select(e => e.Description)));
        }

        return new UserProfileDto
        {
            Id = user.Id,
            Email = user.Email!,
            Role = user.Role.ToString(),

            // Temporary until email service is implemented.
            EmailConfirmationToken = emailToken
        };
    }

    public async Task<bool> DeactivateAccountAsync()
    {
        var user = await GetCurrentUserInternalAsync();

        if (user.IsDeleted)
            throw new InvalidOperationException("Account is already deactivated.");

        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            throw new Exception(
                string.Join(", ",
                    result.Errors.Select(e => e.Description)));
        }

        return true;
    }

    public async Task<bool> ConfirmEmailAsync(string token)
    {
        var user = await GetCurrentUserInternalAsync();

        var result = await _userManager.ConfirmEmailAsync(user, token);

        if (!result.Succeeded)
        {
            throw new Exception(
                string.Join(", ",
                    result.Errors.Select(e => e.Description)));
        }

        return true;
    }
}