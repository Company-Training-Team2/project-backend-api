using System.Linq.Expressions;
using EventHub.Application.DTOs.Common;
using EventHub.Application.DTOs.WorkPost;
using EventHub.Application.Interfaces;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Application.Services;

/// <summary>
/// Audit Module 2 (Home &amp; Discovery):
///  - GET /api/workposts/search  -> SearchAsync
///  - GET /api/workposts/{id}    -> GetDetailAsync
/// Also backs the "recommended vendors" strip on the Home dashboard.
/// </summary>
public class WorkPostService : IWorkPostService
{
    private readonly IUnitOfWork _unitOfWork;

    public WorkPostService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // Shared card-level projection, reused by Search and Featured so the
    // "average rating from reviews" logic only lives in one place.
    private static readonly Expression<Func<WorkPost, WorkPostSummaryDto>> ToSummaryDto = w => new WorkPostSummaryDto
    {
        Id = w.Id,
        Title = w.Title,
        CategoryName = w.Category.Name,
        VendorBusinessName = w.VendorProfile.BusinessName,
        City = w.City,
        Price = w.Price,
        PrimaryImageUrl = w.Images
            .Where(i => i.IsPrimary)
            .Select(i => i.ImageUrl)
            .FirstOrDefault()
            ?? w.Images.Select(i => i.ImageUrl).FirstOrDefault(),
        AverageRating = w.Bookings
            .Select(b => b.Review)
            .Where(r => r != null)
            .Select(r => (double)r!.Rating)
            .DefaultIfEmpty(0)
            .Average(),
        ReviewCount = w.Bookings.Count(b => b.Review != null)
    };

    public async Task<PagedResultDto<WorkPostSummaryDto>> SearchAsync(WorkPostSearchQuery query)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 50 ? 12 : query.PageSize;

        var baseQuery = _unitOfWork.Repository<WorkPost>().Query()
            .Where(w => w.ApprovalStatus == ApprovalStatus.Approved);

        if (!string.IsNullOrWhiteSpace(query.Category))
            baseQuery = baseQuery.Where(w => w.Category.Name == query.Category);

        if (!string.IsNullOrWhiteSpace(query.City))
            baseQuery = baseQuery.Where(w => w.City == query.City);

        if (query.MinPrice.HasValue)
            baseQuery = baseQuery.Where(w => w.Price >= query.MinPrice.Value);

        if (query.MaxPrice.HasValue)
            baseQuery = baseQuery.Where(w => w.Price <= query.MaxPrice.Value);

        if (!string.IsNullOrWhiteSpace(query.Keyword))
            baseQuery = baseQuery.Where(w =>
                w.Title.Contains(query.Keyword) ||
                w.Description.Contains(query.Keyword));

        var projected = baseQuery.Select(ToSummaryDto);

        // Rating is computed (subquery average), so it's filtered after projection.
        if (query.MinRating.HasValue)
            projected = projected.Where(w => w.AverageRating >= query.MinRating.Value);

        var totalCount = await projected.CountAsync();

        var items = await projected
            .OrderByDescending(w => w.AverageRating)
            .ThenBy(w => w.Title)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResultDto<WorkPostSummaryDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<WorkPostDetailDto> GetDetailAsync(int workPostId)
    {
        var detail = await _unitOfWork.Repository<WorkPost>().Query()
            .Where(w => w.Id == workPostId && w.ApprovalStatus == ApprovalStatus.Approved)
            .Select(w => new WorkPostDetailDto
            {
                Id = w.Id,
                Title = w.Title,
                Description = w.Description,
                Price = w.Price,
                City = w.City,
                Address = w.Address,
                CategoryName = w.Category.Name,
                VendorProfileId = w.VendorProfileId,
                VendorBusinessName = w.VendorProfile.BusinessName,
                VendorLogoUrl = w.VendorProfile.LogoUrl,
                VendorIsVerified = w.VendorProfile.IsVerified,
                AverageRating = w.Bookings
                    .Select(b => b.Review)
                    .Where(r => r != null)
                    .Select(r => (double)r!.Rating)
                    .DefaultIfEmpty(0)
                    .Average(),
                ReviewCount = w.Bookings.Count(b => b.Review != null),
                Images = w.Images
                    .OrderByDescending(i => i.IsPrimary)
                    .Select(i => new WorkPostImageDto
                    {
                        Id = i.Id,
                        ImageUrl = i.ImageUrl,
                        IsPrimary = i.IsPrimary
                    })
                    .ToList(),
                ServicePackages = w.ServicePackages
                    .Where(sp => sp.IsActive)
                    .Select(sp => new ServicePackageDto
                    {
                        Id = sp.Id,
                        Name = sp.Name,
                        Description = sp.Description,
                        Price = sp.Price,
                        Includes = sp.Includes
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync();

        if (detail is null)
            throw new Exception("Work post not found.");

        // Customer reviews (kept as a second query: needs Booking -> Customer.FullName,
        // which doesn't sit cleanly inside the WorkPost projection above).
        detail.Reviews = await _unitOfWork.Repository<Review>().Query()
            .Where(r => r.Booking.WorkPostId == workPostId)
            .OrderByDescending(r => r.CreatedAt)
            .Take(20)
            .Select(r => new ReviewSummaryDto
            {
                Id = r.Id,
                CustomerName = r.Booking.Customer.FullName,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();

        // Available time slots: open, future dates only.
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        detail.AvailableTimeSlots = await _unitOfWork.Repository<WorkPostAvailability>().Query()
            .Where(a => a.WorkPostId == workPostId && a.IsAvailable && a.Date >= today)
            .OrderBy(a => a.Date)
            .Take(30)
            .Select(a => new AvailableSlotDto
            {
                Date = a.Date,
                Notes = a.Notes
            })
            .ToListAsync();

        return detail;
    }

    public async Task<IEnumerable<WorkPostSummaryDto>> GetFeaturedAsync(string? city, int take = 6)
    {
        if (take < 1) take = 6;

        var baseQuery = _unitOfWork.Repository<WorkPost>().Query()
            .Where(w => w.ApprovalStatus == ApprovalStatus.Approved);

        var cityQuery = baseQuery;

        if (!string.IsNullOrWhiteSpace(city))
            cityQuery = cityQuery.Where(w => w.City == city);

        var result = await cityQuery
            .Select(ToSummaryDto)
            .OrderByDescending(w => w.AverageRating)
            .ThenByDescending(w => w.ReviewCount)
            .Take(take)
            .ToListAsync();

        // Top up with platform-wide picks if the city filter didn't yield enough.
        if (!string.IsNullOrWhiteSpace(city) && result.Count < take)
        {
            var existingIds = result.Select(r => r.Id).ToList();

            var fallback = await baseQuery
                .Where(w => !existingIds.Contains(w.Id))
                .Select(ToSummaryDto)
                .OrderByDescending(w => w.AverageRating)
                .ThenByDescending(w => w.ReviewCount)
                .Take(take - result.Count)
                .ToListAsync();

            result.AddRange(fallback);
        }

        return result;
    }
}
