using EventHub.Application.DTOs;
using EventHub.Application.Interfaces;
using EventHub.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace EventHub.Application.Services;

public class AdminUserService : IAdminUserService
{
    private readonly UserManager<User> _userManager;

    public AdminUserService(UserManager<User> userManager)
    {
        _userManager = userManager;
    }


    public Task<List<AdminUserDto>> GetUsersAsync(
        string? role,
        bool? isDeleted)
    {
        var users = _userManager.Users.AsQueryable();


        if (!string.IsNullOrEmpty(role))
        {
            users = users.Where(x => x.Role.ToString() == role);
        }


        if (isDeleted.HasValue)
        {
            users = users.Where(x => x.IsDeleted == isDeleted.Value);
        }


        var result = users.Select(x => new AdminUserDto
        {
            Id = x.Id,
            Email = x.Email!,
            Role = x.Role.ToString(),
            IsDeleted = x.IsDeleted,
            DeletedAt = x.DeletedAt
        }).ToList();


        return Task.FromResult(result);
    }


    public async Task<bool> SuspendUserAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user == null)
            return false;


        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;


        return (await _userManager.UpdateAsync(user)).Succeeded;
    }


    public async Task<bool> ActivateUserAsync(int userId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user == null)
            return false;


        user.IsDeleted = false;
        user.DeletedAt = null;


        return (await _userManager.UpdateAsync(user)).Succeeded;
    }
}