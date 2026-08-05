using System.Security.Claims;
using EventHub.Application.DTOs.Home;
using EventHub.Application.Interfaces;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace EventHub.Application.Services;

/// <summary>
/// Audit Module 2: GET /api/home/dashboard aggregated payload for the
/// authenticated customer's Home (Summary Dashboard) screen.
/// </summary>
public class HomeService : IHomeService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IWorkPostService _workPostService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HomeService(
        IUnitOfWork unitOfWork,
        IWorkPostService workPostService,
        IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _workPostService = workPostService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<HomeDashboardDto> GetDashboardAsync()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?
            .User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("User is not authenticated.");

        var profile = await _unitOfWork.Repository<CustomerProfile>()
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile is null)
            throw new Exception("Customer profile not found.");

        var now = DateTime.UtcNow;

        var upcomingEvents = (await _unitOfWork.Repository<Event>()
            .FindAsync(e => e.CustomerId == profile.Id && e.TargetDate >= now))
            .OrderBy(e => e.TargetDate)
            .ToList();

        var nextEvent = upcomingEvents.FirstOrDefault();

        var favoritesCount = await _unitOfWork.Repository<Favorite>()
            .CountAsync(f => f.CustomerId == profile.Id);

        var pendingBookingsCount = await _unitOfWork.Repository<Booking>()
            .CountAsync(b => b.CustomerId == profile.Id && b.Status == BookingStatus.Pending);

        var confirmedBookingsCount = await _unitOfWork.Repository<Booking>()
            .CountAsync(b => b.CustomerId == profile.Id && b.Status == BookingStatus.Confirmed);

        var recommended = await _workPostService.GetFeaturedAsync(profile.City, 6);

        return new HomeDashboardDto
        {
            CustomerName = profile.FullName,
            UpcomingEventsCount = upcomingEvents.Count,
            NextEvent = nextEvent is null
                ? null
                : new UpcomingEventSummaryDto
                {
                    Id = nextEvent.Id,
                    Name = nextEvent.Name,
                    EventType = nextEvent.EventType,
                    TargetDate = nextEvent.TargetDate,
                    DaysRemaining = Math.Max(0, (nextEvent.TargetDate.Date - now.Date).Days)
                },
            FavoritesCount = favoritesCount,
            PendingBookingsCount = pendingBookingsCount,
            ConfirmedBookingsCount = confirmedBookingsCount,
            RecommendedWorkPosts = recommended.ToList()
        };
    }
}
