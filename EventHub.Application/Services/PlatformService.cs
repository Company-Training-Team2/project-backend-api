using EventHub.Application.DTOs.Platform;
using EventHub.Application.Interfaces;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Application.Services;

/// <summary>
/// Audit Module 2: GET /api/platform/stats — public, unauthenticated
/// marketing metrics.
/// </summary>
public class PlatformService : IPlatformService
{
    private readonly IUnitOfWork _unitOfWork;

    public PlatformService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PlatformStatsDto> GetStatsAsync()
    {
        var totalVendors = await _unitOfWork.Repository<VendorProfile>()
            .CountAsync(v => v.ApprovalStatus == ApprovalStatus.Approved);

        var totalCustomers = await _unitOfWork.Repository<CustomerProfile>()
            .CountAsync();

        var totalWorkPosts = await _unitOfWork.Repository<WorkPost>()
            .CountAsync(w => w.ApprovalStatus == ApprovalStatus.Approved);

        var totalCategories = await _unitOfWork.Repository<Category>()
            .CountAsync();

        var totalCompletedBookings = await _unitOfWork.Repository<Booking>()
            .CountAsync(b => b.Status == BookingStatus.Completed);

        var totalEventsPlanned = await _unitOfWork.Repository<Event>()
            .CountAsync();

        var averageRating = await _unitOfWork.Repository<Review>().Query()
            .Select(r => (double)r.Rating)
            .DefaultIfEmpty(0)
            .AverageAsync();

        return new PlatformStatsDto
        {
            TotalVendors = totalVendors,
            TotalCustomers = totalCustomers,
            TotalApprovedWorkPosts = totalWorkPosts,
            TotalCategories = totalCategories,
            TotalCompletedBookings = totalCompletedBookings,
            TotalEventsPlanned = totalEventsPlanned,
            AveragePlatformRating = Math.Round(averageRating, 2)
        };
    }
}
