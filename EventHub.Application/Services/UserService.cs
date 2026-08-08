using EventHub.Application.DTOs.User;
using EventHub.Application.Interfaces;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace EventHub.Application.Services;

public class UserService : IUserService
{
    private readonly UserManager<User> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUnitOfWork _unitOfWork;

    public UserService(
     UserManager<User> userManager,
     IHttpContextAccessor httpContextAccessor,
     IUnitOfWork unitOfWork)
    {
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
        _unitOfWork = unitOfWork;
    }

    // ─── Internal helper ──────────────────────────────────────────────────────
    private async Task<User> GetAuthenticatedUserAsync()
    {
        var userId = _httpContextAccessor.HttpContext?
            .User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
            throw new UnauthorizedAccessException("User is not authenticated.");

        var user = await _userManager.FindByIdAsync(userId);

        if (user == null || user.IsDeleted)
            throw new UnauthorizedAccessException("User not found or deactivated.");

        return user;
    }

    // ═══════════════════════════════════════════════════════════
    // GET /api/users/me  (audit Module 12: fix mock data bug)
    // ═══════════════════════════════════════════════════════════
    public async Task<UserProfileDto> GetCurrentUserAsync()
    {
        var user = await GetAuthenticatedUserAsync();
        var dto = BuildBaseDto(user);

        // Enrich with role-specific profile data
        if (user.Role == UserRole.Customer)
        {
            var profile = await _unitOfWork.Repository<CustomerProfile>()
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (profile != null)
            {
                dto.FullName = profile.FullName;
                dto.PhoneNumber = profile.PhoneNumber;
                dto.City = profile.City;
                dto.AvatarUrl = profile.AvatarUrl;
            }
        }
        else if (user.Role == UserRole.Vendor)
        {
            var profile = await _unitOfWork.Repository<VendorProfile>()
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (profile != null)
            {
                dto.BusinessName = profile.BusinessName;
                dto.BioDescription = profile.BioDescription;
                dto.ApprovalStatus = profile.ApprovalStatus.ToString();
            }
        }

        return dto;
    }

    // ═══════════════════════════════════════════════════════════
    // PUT /api/users/me
    // ═══════════════════════════════════════════════════════════
    public async Task<UserProfileDto> UpdateProfileAsync(UpdateUserDto dto)
    {
        var user = await GetAuthenticatedUserAsync();

        // ── Email change ───────────────────────────────────────────────────────
        if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
        {
            var taken = await _userManager.FindByEmailAsync(dto.Email);
            if (taken != null)
                throw new InvalidOperationException("Email is already in use.");

            user.Email = dto.Email;
            user.UserName = dto.Email;
            user.EmailConfirmed = false;
            user.IsEmailVerified = false;
            // TODO: trigger new OTP verification email
        }

        user.UpdatedAt = DateTime.UtcNow;
        var identityResult = await _userManager.UpdateAsync(user);
        if (!identityResult.Succeeded)
            throw new InvalidOperationException(string.Join(", ", identityResult.Errors.Select(e => e.Description)));

        // ── Update Customer profile ────────────────────────────────────────────
        if (user.Role == UserRole.Customer)
        {
            var profile = await _unitOfWork.Repository<CustomerProfile>()
                .FirstOrDefaultAsync(p => p.UserId == user.Id);

            if (profile != null)
            {
                if (dto.FullName != null) profile.FullName = dto.FullName;
                if (dto.PhoneNumber != null) profile.PhoneNumber = dto.PhoneNumber;
                if (dto.City != null) profile.City = dto.City;
                if (dto.AvatarUrl != null) profile.AvatarUrl = dto.AvatarUrl;
                profile.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Repository<CustomerProfile>().Update(profile);
            }
        }

        await _unitOfWork.SaveChangesAsync();

        return await GetCurrentUserAsync();
    }

    // ═══════════════════════════════════════════════════════════
    // PUT /api/users/me/password  (audit Module 12)
    // ═══════════════════════════════════════════════════════════
    public async Task ChangePasswordAsync(ChangePasswordDto dto)
    {
        var user = await GetAuthenticatedUserAsync();

        var result = await _userManager.ChangePasswordAsync(
            user,
            dto.CurrentPassword,
            dto.NewPassword);

        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));

        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);
    }

    // ═══════════════════════════════════════════════════════════
    // DELETE /api/users/me/deactivate
    // ═══════════════════════════════════════════════════════════
    public async Task DeactivateAccountAsync()
    {
        var user = await GetAuthenticatedUserAsync();

        if (user.IsDeleted)
            throw new InvalidOperationException("Account is already deactivated.");

        user.IsDeleted = true;
        user.IsActive = false;
        user.DeletedAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw new InvalidOperationException(string.Join(", ", result.Errors.Select(e => e.Description)));
    }

    // ═══════════════════════════════════════════════════════════
    // GET /api/users/me/activity  (audit Module 12: missing endpoint)
    // ═══════════════════════════════════════════════════════════
    public async Task<IEnumerable<UserActivityDto>> GetActivityLogAsync(int pageNumber, int pageSize)
    {
        // TODO: implement a UserAuditLog entity for full activity tracking.
        // For now, return an empty list so the endpoint is wired and won't 404.
        await Task.CompletedTask;
        return Enumerable.Empty<UserActivityDto>();
    }

    // ─── Private helpers ──────────────────────────────────────────────────────
    private static UserProfileDto BuildBaseDto(User user) => new()
    {
        Id = user.Id,
        Email = user.Email!,
        Role = user.Role.ToString(),
        IsEmailVerified = user.IsEmailVerified,
        IsActive = user.IsActive,
        CreatedAt = user.CreatedAt
    };
}