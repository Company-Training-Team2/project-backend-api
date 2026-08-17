using EventHub.Application.DTOs.Notification;
using EventHub.Application.DTOs.Vendor;
using EventHub.Application.DTOs.WorkPost;
using Microsoft.AspNetCore.Http;
using EventHub.Application.Interfaces;
using EventHub.Domain.Entities;
using EventHub.Domain.Enums;
using EventHub.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Application.Services;

public class VendorService : IVendorService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly IPayoutService _payoutService;

    public VendorService(
        IUnitOfWork unitOfWork,
        INotificationService notificationService,
        IPayoutService payoutService)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _payoutService = payoutService;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Dashboard
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<VendorDashboardDto> GetDashboardAsync(int userId)
    {
        var vendor = await GetVendorProfileOrThrowAsync(userId);

        var workPostIds = await _unitOfWork.Repository<WorkPost>()
            .Query()
            .Where(w => w.VendorProfileId == vendor.Id)
            .Select(w => w.Id)
            .ToListAsync();

        var bookings = await _unitOfWork.Repository<Booking>()
            .Query()
            .Where(b => workPostIds.Contains(b.WorkPostId))
            .Include(b => b.WorkPost)
            .Include(b => b.Customer)
            .ToListAsync();

        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var completedBookings = bookings.Where(b => b.Status == BookingStatus.Completed).ToList();
        var revenue = completedBookings.Sum(b => b.TotalPrice);
        var monthRevenue = completedBookings
            .Where(b => b.CreatedAt >= monthStart)
            .Sum(b => b.TotalPrice);

        var reviews = await _unitOfWork.Repository<Review>()
            .Query()
            .Where(r => workPostIds.Contains(r.Booking.WorkPostId))
            .ToListAsync();

        var avgRating = reviews.Count > 0 ? reviews.Average(r => (double)r.Rating) : 0;

        var today = DateOnly.FromDateTime(now);

        var upcoming = bookings
            .Where(b => b.BookingDate >= today &&
                        b.Status is BookingStatus.Pending or BookingStatus.Accepted)
            .OrderBy(b => b.BookingDate)
            .Take(5)
            .Select(b => new UpcomingVendorBookingDto
            {
                BookingId = b.Id,
                CustomerName = b.Customer?.FullName ?? "Unknown",
                WorkPostTitle = b.WorkPost.Title,
                BookingDate = b.BookingDate,
                TotalPrice = b.TotalPrice,
                Status = b.Status.ToString()
            })
            .ToList();

        return new VendorDashboardDto
        {
            TotalBookings = bookings.Count,
            PendingBookings = bookings.Count(b => b.Status == BookingStatus.Pending),
            ConfirmedBookings = bookings.Count(b => b.Status == BookingStatus.Accepted),
            CompletedBookings = completedBookings.Count,
            TotalRevenue = revenue,
            MonthRevenue = monthRevenue,
            AverageRating = Math.Round(avgRating, 2),
            ReviewCount = reviews.Count,
            TotalWorkPosts = workPostIds.Count,
            UpcomingBookings = upcoming
        };
    }

    // ─────────────────────────────────────────────────────────────────────────
    // WorkPost CRUD
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<IEnumerable<VendorWorkPostDto>> GetMyWorkPostsAsync(int userId)
    {
        var vendor = await GetVendorProfileOrThrowAsync(userId);

        return await _unitOfWork.Repository<WorkPost>()
            .Query()
            .Where(w => w.VendorProfileId == vendor.Id)
            .Select(w => MapToVendorWorkPostDto(w))
            .ToListAsync();
    }

    public async Task<VendorWorkPostDto> GetWorkPostByIdAsync(int userId, int workPostId)
    {
        var vendor = await GetVendorProfileOrThrowAsync(userId);

        var workPost = await _unitOfWork.Repository<WorkPost>()
            .Query()
            .Where(w => w.Id == workPostId && w.VendorProfileId == vendor.Id)
            .Select(w => MapToVendorWorkPostDto(w))
            .FirstOrDefaultAsync();

        if (workPost is null)
            throw new Exception("WorkPost not found or does not belong to this vendor.");

        return workPost;
    }

    public async Task<VendorWorkPostDto> CreateWorkPostAsync(int userId, CreateWorkPostDto dto)
    {
        var vendor = await GetVendorProfileOrThrowAsync(userId);

        if (vendor.ApprovalStatus != ApprovalStatus.Approved)
            throw new Exception("Your vendor account is not yet approved.");

        var workPost = new WorkPost
        {
            VendorProfileId = vendor.Id,
            CategoryId = dto.CategoryId,
            Title = dto.Title,
            Description = dto.Description,
            Price = dto.Price,
            City = dto.City,
            Address = dto.Address,
            ApprovalStatus = ApprovalStatus.Pending   // admin must approve new listings
        };

        foreach (var pkg in dto.ServicePackages)
        {
            workPost.ServicePackages.Add(new ServicePackage
            {
                Name = pkg.Name,
                Description = pkg.Description,
                Price = pkg.Price,
                Includes = pkg.Includes,
                IsActive = true
            });
        }

        await _unitOfWork.Repository<WorkPost>().AddAsync(workPost);
        await _unitOfWork.SaveChangesAsync();

        return await GetWorkPostByIdAsync(userId, workPost.Id);
    }

    public async Task<VendorWorkPostDto> UpdateWorkPostAsync(int userId, int workPostId, UpdateWorkPostDto dto)
    {
        var vendor = await GetVendorProfileOrThrowAsync(userId);

        var workPost = await _unitOfWork.Repository<WorkPost>()
            .Query()
            .FirstOrDefaultAsync(w => w.Id == workPostId && w.VendorProfileId == vendor.Id);

        if (workPost is null)
            throw new Exception("WorkPost not found or does not belong to this vendor.");

        if (dto.CategoryId.HasValue) workPost.CategoryId = dto.CategoryId.Value;
        if (dto.Title is not null) workPost.Title = dto.Title;
        if (dto.Description is not null) workPost.Description = dto.Description;
        if (dto.Price.HasValue) workPost.Price = dto.Price.Value;
        if (dto.City is not null) workPost.City = dto.City;
        if (dto.Address is not null) workPost.Address = dto.Address;

        _unitOfWork.Repository<WorkPost>().Update(workPost);
        await _unitOfWork.SaveChangesAsync();

        return await GetWorkPostByIdAsync(userId, workPostId);
    }

    public async Task DeleteWorkPostAsync(int userId, int workPostId)
    {
        var vendor = await GetVendorProfileOrThrowAsync(userId);

        var workPost = await _unitOfWork.Repository<WorkPost>()
            .Query()
            .FirstOrDefaultAsync(w => w.Id == workPostId && w.VendorProfileId == vendor.Id);

        if (workPost is null)
            throw new Exception("WorkPost not found or does not belong to this vendor.");

        _unitOfWork.Repository<WorkPost>().Delete(workPost);
        await _unitOfWork.SaveChangesAsync();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // WorkPost Images
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// POST /api/vendor/services/{id}/images
    ///
    /// Stores each uploaded image against the WorkPost. The actual blob write
    /// is delegated to the infrastructure blob handler. Until a production blob
    /// service is wired this method generates a deterministic placeholder URL
    /// so the rest of the flow (DB persist, response) is fully exercisable.
    ///
    /// Blob infrastructure hook: replace the <c>ResolveImageUrlAsync</c> helper
    /// below with a call to <c>IBlobService.UploadAsync</c> (or equivalent) once
    /// the Azure Blob / S3 adapter is in place.
    /// </summary>
    public async Task<IEnumerable<WorkPostImageDto>> UploadWorkPostImagesAsync(
        int userId,
        int workPostId,
        UploadWorkPostImagesRequest request)
    {
        var vendor = await GetVendorProfileOrThrowAsync(userId);

        // Ownership check
        var workPost = await _unitOfWork.Repository<WorkPost>()
            .Query()
            .Include(w => w.Images)
            .FirstOrDefaultAsync(w => w.Id == workPostId && w.VendorProfileId == vendor.Id);

        if (workPost is null)
            throw new InvalidOperationException(
                "WorkPost not found or does not belong to this vendor.");

        if (request.Images is null || request.Images.Count == 0)
            throw new InvalidOperationException("No image files were provided.");

        var hasPrimary = workPost.Images.Any(i => i.IsPrimary);
        var saved      = new List<WorkPostImageDto>();

        for (var i = 0; i < request.Images.Count; i++)
        {
            var file      = request.Images[i];
            var imageUrl  = await ResolveImageUrlAsync(file, workPostId);
            var isPrimary = !hasPrimary && request.SetFirstAsPrimary && i == 0;

            // If promoting the first image to primary, demote any existing primary
            if (isPrimary && workPost.Images.Any(img => img.IsPrimary))
            {
                foreach (var existing in workPost.Images.Where(img => img.IsPrimary))
                {
                    existing.IsPrimary = false;
                    _unitOfWork.Repository<WorkPostImage>().Update(existing);
                }
            }

            var entity = new WorkPostImage
            {
                WorkPostId  = workPostId,
                ImageUrl    = imageUrl,
                IsPrimary   = isPrimary,
                UploadedAt  = DateTime.UtcNow
            };

            await _unitOfWork.Repository<WorkPostImage>().AddAsync(entity);
            hasPrimary = hasPrimary || isPrimary;

            saved.Add(new WorkPostImageDto
            {
                Id        = entity.Id,   // populated after SaveChanges
                ImageUrl  = entity.ImageUrl,
                IsPrimary = entity.IsPrimary
            });
        }

        await _unitOfWork.SaveChangesAsync();

        // Re-fetch with auto-generated Ids
        var persisted = await _unitOfWork.Repository<WorkPostImage>()
            .Query()
            .Where(img => img.WorkPostId == workPostId)
            .OrderByDescending(img => img.IsPrimary)
            .ThenByDescending(img => img.UploadedAt)
            .Select(img => new WorkPostImageDto
            {
                Id        = img.Id,
                ImageUrl  = img.ImageUrl,
                IsPrimary = img.IsPrimary
            })
            .ToListAsync();

        return persisted;
    }

    /// <summary>
    /// Blob infrastructure hook.
    /// Swap this for a real <c>IBlobService.UploadAsync(stream, fileName)</c>
    /// call once the storage adapter is connected. The placeholder URL embeds
    /// the filename so it is still unique and human-readable in dev/test.
    /// </summary>
    private static Task<string> ResolveImageUrlAsync(IFormFile file, int workPostId)
    {
        // TODO: replace with actual blob upload when IBlobService is available.
        var ext         = Path.GetExtension(file.FileName).ToLowerInvariant();
        var safeName    = Path.GetFileNameWithoutExtension(file.FileName)
                              .Replace(" ", "-")
                              .ToLowerInvariant();
        var placeholder = $"/uploads/workposts/{workPostId}/{safeName}-{Guid.NewGuid():N}{ext}";
        return Task.FromResult(placeholder);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Availability
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<IEnumerable<VendorAvailabilityDto>> GetAvailabilityAsync(int userId)
    {
        var vendor = await GetVendorProfileOrThrowAsync(userId);

        var workPosts = await _unitOfWork.Repository<WorkPost>()
            .Query()
            .Where(w => w.VendorProfileId == vendor.Id)
            .Include(w => w.Availabilities)
            .ToListAsync();

        return workPosts.Select(w => new VendorAvailabilityDto
        {
            WorkPostId = w.Id,
            WorkPostTitle = w.Title,
            Days = w.Availabilities
                .OrderBy(a => a.Date)
                .Select(a => new AvailabilityDayDto
                {
                    Id = a.Id,
                    Date = a.Date,
                    IsAvailable = a.IsAvailable,
                    Notes = a.Notes
                })
                .ToList()
        });
    }

    public async Task UpdateAvailabilityAsync(int userId, UpdateAvailabilityDto dto)
    {
        var vendor = await GetVendorProfileOrThrowAsync(userId);

        // Ownership check
        var workPost = await _unitOfWork.Repository<WorkPost>()
            .Query()
            .FirstOrDefaultAsync(w => w.Id == dto.WorkPostId && w.VendorProfileId == vendor.Id);

        if (workPost is null)
            throw new Exception("WorkPost not found or does not belong to this vendor.");

        var existingAvailabilities = await _unitOfWork.Repository<WorkPostAvailability>()
            .Query()
            .Where(a => a.WorkPostId == dto.WorkPostId)
            .ToListAsync();

        foreach (var dayUpdate in dto.Days)
        {
            var existing = existingAvailabilities
                .FirstOrDefault(a => a.Date == dayUpdate.Date);

            if (existing is not null)
            {
                existing.IsAvailable = dayUpdate.IsAvailable;
                existing.Notes = dayUpdate.Notes;
                _unitOfWork.Repository<WorkPostAvailability>().Update(existing);
            }
            else
            {
                await _unitOfWork.Repository<WorkPostAvailability>().AddAsync(new WorkPostAvailability
                {
                    WorkPostId = dto.WorkPostId,
                    Date = dayUpdate.Date,
                    IsAvailable = dayUpdate.IsAvailable,
                    Notes = dayUpdate.Notes
                });
            }
        }

        await _unitOfWork.SaveChangesAsync();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Bookings
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<IEnumerable<VendorBookingDto>> GetBookingsAsync(int userId, string? status)
    {
        var vendor = await GetVendorProfileOrThrowAsync(userId);

        var query = _unitOfWork.Repository<Booking>()
            .Query()
            .Where(b => b.WorkPost.VendorProfileId == vendor.Id)
            .Include(b => b.WorkPost)
            .Include(b => b.Customer)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<BookingStatus>(status, ignoreCase: true, out var parsedStatus))
        {
            query = query.Where(b => b.Status == parsedStatus);
        }

        var bookings = await query
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        return bookings.Select(MapToVendorBookingDto);
    }

    public async Task<VendorBookingDto> ApproveBookingAsync(int userId, int bookingId)
    {
        var vendor = await GetVendorProfileOrThrowAsync(userId);

        var booking = await _unitOfWork.Repository<Booking>()
            .Query()
            .Include(b => b.WorkPost)
            .Include(b => b.Customer)
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.WorkPost.VendorProfileId == vendor.Id);

        if (booking is null)
            throw new Exception("Booking not found or does not belong to this vendor.");

        if (booking.Status != BookingStatus.Pending)
            throw new Exception("Only pending bookings can be approved.");

        booking.Status = BookingStatus.Accepted;
        _unitOfWork.Repository<Booking>().Update(booking);
        await _unitOfWork.SaveChangesAsync();

        // Notify customer
        await _notificationService.NotifyAsync(new CreateNotificationDto
        {
            UserId = booking.Customer.UserId,
            Type = NotificationType.BookingStatusUpdate,
            Title = "Booking Confirmed",
            Body = $"Your booking for \"{booking.WorkPost.Title}\" has been approved.",
            RelatedEntityId = booking.Id
        });

        return MapToVendorBookingDto(booking);
    }

    public async Task<VendorBookingDto> DeclineBookingAsync(int userId, int bookingId)
    {
        var vendor = await GetVendorProfileOrThrowAsync(userId);

        var booking = await _unitOfWork.Repository<Booking>()
            .Query()
            .Include(b => b.WorkPost)
            .Include(b => b.Customer)
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.WorkPost.VendorProfileId == vendor.Id);

        if (booking is null)
            throw new Exception("Booking not found or does not belong to this vendor.");

        if (booking.Status != BookingStatus.Pending)
            throw new Exception("Only pending bookings can be declined.");

        booking.Status = BookingStatus.Rejected;
        _unitOfWork.Repository<Booking>().Update(booking);

        // Restore availability
        var availability = await _unitOfWork.Repository<WorkPostAvailability>()
            .FirstOrDefaultAsync(a =>
                a.WorkPostId == booking.WorkPostId && a.Date == booking.BookingDate);

        if (availability is not null)
        {
            availability.IsAvailable = true;
            _unitOfWork.Repository<WorkPostAvailability>().Update(availability);
        }

        await _unitOfWork.SaveChangesAsync();

        // Notify customer
        await _notificationService.NotifyAsync(new CreateNotificationDto
        {
            UserId = booking.Customer.UserId,
            Type = NotificationType.BookingStatusUpdate,
            Title = "Booking Declined",
            Body = $"Your booking for \"{booking.WorkPost.Title}\" was declined by the vendor.",
            RelatedEntityId = booking.Id
        });

        return MapToVendorBookingDto(booking);
    }

    /// <summary>
    /// PUT /api/vendor/bookings/{id}/complete — vendor marks a Paid booking as
    /// delivered. Per the Payment module spec, the actual bank transfer to the
    /// vendor is only created once the event/service is Completed (not at
    /// payment time), so this is also the trigger point for payout creation.
    /// </summary>
    public async Task<VendorBookingDto> CompleteBookingAsync(int userId, int bookingId)
    {
        var vendor = await GetVendorProfileOrThrowAsync(userId);

        var booking = await _unitOfWork.Repository<Booking>()
            .Query()
            .Include(b => b.WorkPost)
            .Include(b => b.Customer)
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.WorkPost.VendorProfileId == vendor.Id);

        if (booking is null)
            throw new Exception("Booking not found or does not belong to this vendor.");

        if (booking.Status != BookingStatus.Paid)
            throw new Exception("Only paid bookings can be marked as completed.");

        booking.Status = BookingStatus.Completed;
        _unitOfWork.Repository<Booking>().Update(booking);

        await _unitOfWork.SaveChangesAsync();

        // Notify customer
        await _notificationService.NotifyAsync(new CreateNotificationDto
        {
            UserId = booking.Customer.UserId,
            Type = NotificationType.BookingStatusUpdate,
            Title = "Booking Completed",
            Body = $"Your booking for \"{booking.WorkPost.Title}\" has been marked as completed.",
            RelatedEntityId = booking.Id
        });

        // Best-effort: create the vendor's due Payout now that the booking is
        // Completed. A failure here shouldn't fail the completion action itself —
        // the admin can always trigger ProcessDuePayoutsAsync manually as a fallback.
        try
        {
            await _payoutService.ProcessDuePayoutsAsync();
        }
        catch
        {
            // Swallow: payout creation failure must not roll back the booking completion.
        }

        return MapToVendorBookingDto(booking);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Analytics
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<VendorAnalyticsDto> GetAnalyticsAsync(int userId)
    {
        var vendor = await GetVendorProfileOrThrowAsync(userId);

        var workPostIds = await _unitOfWork.Repository<WorkPost>()
            .Query()
            .Where(w => w.VendorProfileId == vendor.Id)
            .Select(w => w.Id)
            .ToListAsync();

        var bookings = await _unitOfWork.Repository<Booking>()
            .Query()
            .Where(b => workPostIds.Contains(b.WorkPostId))
            .Include(b => b.WorkPost)
            .ToListAsync();

        var reviews = await _unitOfWork.Repository<Review>()
            .Query()
            .Where(r => workPostIds.Contains(r.Booking.WorkPostId))
            .ToListAsync();

        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var completed = bookings.Where(b => b.Status == BookingStatus.Completed).ToList();

        var monthly = completed
            .GroupBy(b => new { b.CreatedAt.Year, b.CreatedAt.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .Select(g => new MonthlyRevenueDto
            {
                Year = g.Key.Year,
                Month = g.Key.Month,
                Revenue = g.Sum(b => b.TotalPrice),
                BookingCount = g.Count()
            })
            .ToList();

        var wpPerf = workPostIds.Select(id =>
        {
            var wpBookings = completed.Where(b => b.WorkPostId == id).ToList();
            var wpReviews = reviews.Where(r => r.Booking.WorkPostId == id).ToList();
            var title = bookings.FirstOrDefault(b => b.WorkPostId == id)?.WorkPost.Title ?? string.Empty;

            return new WorkPostPerformanceDto
            {
                WorkPostId = id,
                Title = title,
                TotalBookings = wpBookings.Count,
                TotalRevenue = wpBookings.Sum(b => b.TotalPrice),
                AverageRating = wpReviews.Count > 0
                    ? Math.Round(wpReviews.Average(r => (double)r.Rating), 2)
                    : 0
            };
        }).ToList();

        double conversion = bookings.Count > 0
            ? Math.Round((double)completed.Count / bookings.Count * 100, 2)
            : 0;

        return new VendorAnalyticsDto
        {
            TotalRevenue = completed.Sum(b => b.TotalPrice),
            RevenueThisMonth = completed.Where(b => b.CreatedAt >= monthStart).Sum(b => b.TotalPrice),
            TotalBookings = bookings.Count,
            CompletedBookings = completed.Count,
            ConversionRate = conversion,
            AverageRating = reviews.Count > 0
                ? Math.Round(reviews.Average(r => (double)r.Rating), 2)
                : 0,
            ReviewCount = reviews.Count,
            MonthlyRevenue = monthly,
            WorkPostPerformance = wpPerf
        };
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Profile
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<VendorProfileDto> GetProfileAsync(int userId)
    {
        var vendor = await GetVendorProfileOrThrowAsync(userId);
        return MapToProfileDto(vendor);
    }

    public async Task<VendorProfileDto> UpdateProfileAsync(int userId, UpdateVendorProfileDto dto)
    {
        var vendor = await GetVendorProfileOrThrowAsync(userId);

        if (dto.BusinessName is not null) vendor.BusinessName = dto.BusinessName;
        if (dto.BioDescription is not null) vendor.BioDescription = dto.BioDescription;
        if (dto.PhoneNumber is not null) vendor.PhoneNumber = dto.PhoneNumber;
        if (dto.City is not null) vendor.City = dto.City;
        if (dto.LogoUrl is not null) vendor.LogoUrl = dto.LogoUrl;
        if (dto.BankName is not null) vendor.BankName = dto.BankName;
        if (dto.AccountName is not null) vendor.AccountName = dto.AccountName;
        if (dto.AccountNumber is not null) vendor.AccountNumber = dto.AccountNumber;

        _unitOfWork.Repository<VendorProfile>().Update(vendor);
        await _unitOfWork.SaveChangesAsync();

        return MapToProfileDto(vendor);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Verification documents (admin KYC review)
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<string?> GetVerificationDocumentPathAsync(int vendorProfileId, string documentType)
    {
        var vendor = await _unitOfWork.Repository<VendorProfile>().GetByIdAsync(vendorProfileId);
        if (vendor is null) return null;

        return documentType switch
        {
            "commercial-registration" => vendor.CommercialRegistrationPath,
            "national-id" => vendor.NationalIdPath,
            "business-license" => vendor.BusinessLicensePath,
            _ => null
        };
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Private helpers
    // ─────────────────────────────────────────────────────────────────────────

    private async Task<VendorProfile> GetVendorProfileOrThrowAsync(int userId)
    {
        var vendor = await _unitOfWork.Repository<VendorProfile>()
            .FirstOrDefaultAsync(v => v.UserId == userId);

        if (vendor is null)
            throw new UnauthorizedAccessException("Vendor profile not found for this user.");

        return vendor;
    }

    private static VendorProfileDto MapToProfileDto(VendorProfile v) => new()
    {
        Id = v.Id,
        BusinessName = v.BusinessName,
        BioDescription = v.BioDescription,
        PhoneNumber = v.PhoneNumber,
        City = v.City,
        LogoUrl = v.LogoUrl,
        IsVerified = v.IsVerified,
        ApprovalStatus = v.ApprovalStatus.ToString(),
        BankName = v.BankName,
        AccountName = v.AccountName,
        AccountNumber = v.AccountNumber
    };

    private static VendorBookingDto MapToVendorBookingDto(Booking b) => new()
    {
        Id = b.Id,
        WorkPostId = b.WorkPostId,
        WorkPostTitle = b.WorkPost?.Title ?? string.Empty,
        CustomerName = b.Customer?.FullName ?? string.Empty,
        CustomerPhone = b.Customer?.PhoneNumber,
        BookingDate = b.BookingDate,
        Quantity = b.Quantity,
        TotalPrice = b.TotalPrice,
        Status = b.Status.ToString(),
        Notes = b.Notes,
        CreatedAt = b.CreatedAt
    };

    private static VendorWorkPostDto MapToVendorWorkPostDto(WorkPost w) => new()
    {
        Id = w.Id,
        Title = w.Title,
        Description = w.Description,
        Price = w.Price,
        City = w.City,
        Address = w.Address,
        CategoryName = w.Category?.Name ?? string.Empty,
        ApprovalStatus = w.ApprovalStatus.ToString(),
        AverageRating = w.Bookings.Any(b => b.Review != null)
            ? w.Bookings.Where(b => b.Review != null)
                        .Average(b => (double)b.Review!.Rating)
            : 0,
        ReviewCount = w.Bookings.Count(b => b.Review != null),
        TotalBookings = w.Bookings.Count,
        Images = w.Images.OrderByDescending(i => i.IsPrimary)
                         .Select(i => new WorkPostImageDto
                         {
                             Id = i.Id,
                             ImageUrl = i.ImageUrl,
                             IsPrimary = i.IsPrimary
                         }).ToList(),
        ServicePackages = w.ServicePackages.Where(sp => sp.IsActive)
                           .Select(sp => new ServicePackageDto
                           {
                               Id = sp.Id,
                               Name = sp.Name,
                               Description = sp.Description,
                               Price = sp.Price,
                               Includes = sp.Includes
                           }).ToList()
    };
}