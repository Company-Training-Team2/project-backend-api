using System.Security.Claims;
using EventHub.Application.DTOs.Favorite;
using EventHub.Application.Interfaces;
using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace EventHub.Application.Services;

/// <summary>
/// Audit Module 9 (Favorites):
///  - POST /api/favorites/toggle -> ToggleAsync
///  - GET  /api/favorites        -> GetMyFavoritesAsync
/// Backs the global Heart-icon toggle across Vendor Cards / Search views and
/// the consolidated Favorites dashboard.
/// </summary>
public class FavoriteService : IFavoriteService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWorkPostService _workPostService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public FavoriteService(
        IUnitOfWork unitOfWork,
        IWorkPostService workPostService,
        IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _workPostService = workPostService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ToggleFavoriteResultDto> ToggleAsync(int workPostId)
    {
        var profile = await GetCurrentCustomerProfileAsync();

        var workPost = await _unitOfWork.Repository<WorkPost>().GetByIdAsync(workPostId);

        if (workPost is null)
            throw new Exception("Work post not found.");

        var existing = await _unitOfWork.Repository<Favorite>()
            .FirstOrDefaultAsync(f => f.CustomerId == profile.Id && f.WorkPostId == workPostId);

        if (existing is not null)
        {
            _unitOfWork.Repository<Favorite>().Delete(existing);

            await _unitOfWork.SaveChangesAsync();

            return new ToggleFavoriteResultDto { WorkPostId = workPostId, IsFavorited = false };
        }

        var favorite = new Favorite
        {
            CustomerId = profile.Id,
            WorkPostId = workPostId
        };

        await _unitOfWork.Repository<Favorite>().AddAsync(favorite);

        await _unitOfWork.SaveChangesAsync();

        return new ToggleFavoriteResultDto { WorkPostId = workPostId, IsFavorited = true };
    }

    public async Task<IEnumerable<FavoriteDto>> GetMyFavoritesAsync()
    {
        var profile = await GetCurrentCustomerProfileAsync();

        var favorites = (await _unitOfWork.Repository<Favorite>()
            .FindAsync(f => f.CustomerId == profile.Id))
            .ToList();

        if (favorites.Count == 0)
            return Enumerable.Empty<FavoriteDto>();

        var workPostIds = favorites.Select(f => f.WorkPostId);

        var summaries = (await _workPostService.GetSummariesByIdsAsync(workPostIds))
            .ToDictionary(s => s.Id);

        // Favorites whose WorkPost was removed/unapproved since being saved are
        // silently dropped from the dashboard rather than surfaced as broken cards.
        return favorites
            .Where(f => summaries.ContainsKey(f.WorkPostId))
            .Select(f => new FavoriteDto
            {
                FavoriteId = f.Id,
                WorkPost = summaries[f.WorkPostId]
            });
    }

    private async Task<CustomerProfile> GetCurrentCustomerProfileAsync()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?
            .User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("User is not authenticated.");

        var profile = await _unitOfWork.Repository<CustomerProfile>()
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile is null)
            throw new Exception("Customer profile not found.");

        return profile;
    }
}
