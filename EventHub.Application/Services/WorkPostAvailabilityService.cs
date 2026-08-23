using System.Security.Claims;
using EventHub.Application.DTOs.WorkPostAvailability;
using EventHub.Application.Interfaces;
using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;
using Microsoft.AspNetCore.Http;

namespace EventHub.Application.Services;

public class WorkPostAvailabilityService : IWorkPostAvailabilityService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public WorkPostAvailabilityService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
    {
        _unitOfWork = unitOfWork;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<WorkPostAvailabilityDto> CreateAsync(CreateWorkPostAvailabilityDto dto)
    {
        var workPost = await _unitOfWork.Repository<WorkPost>()
            .GetByIdAsync(dto.WorkPostId);

        if (workPost is null)
            throw new Exception("Work Post not found.");

        // Controller had no [Authorize] at all — anyone could create, edit, or
        // delete availability slots for any vendor's WorkPost (e.g. marking a
        // competitor's calendar fully booked). [Authorize(Roles="Vendor")] on
        // the controller now requires a vendor login; this confirms the
        // *specific* WorkPost being touched actually belongs to that vendor.
        await EnsureCurrentUserOwnsWorkPostAsync(workPost);

        var exists = await _unitOfWork.Repository<WorkPostAvailability>()
            .AnyAsync(x => x.WorkPostId == dto.WorkPostId && x.Date == dto.Date);

        if (exists)
            throw new Exception("This date already exists.");

        var availability = new WorkPostAvailability
        {
            WorkPostId = dto.WorkPostId,
            Date = dto.Date,
            IsAvailable = dto.IsAvailable,
            Notes = dto.Notes
        };

        await _unitOfWork.Repository<WorkPostAvailability>()
            .AddAsync(availability);

        await _unitOfWork.SaveChangesAsync();

        return new WorkPostAvailabilityDto
        {
            Id = availability.Id,
            WorkPostId = availability.WorkPostId,
            Date = availability.Date,
            IsAvailable = availability.IsAvailable,
            Notes = availability.Notes
        };
    }

    public async Task<WorkPostAvailabilityDto> UpdateAsync(int id, UpdateWorkPostAvailabilityDto dto)
    {
        var availability = await _unitOfWork.Repository<WorkPostAvailability>()
            .GetByIdAsync(id);

        if (availability is null)
            throw new Exception("Availability not found.");

        var workPost = await _unitOfWork.Repository<WorkPost>()
            .GetByIdAsync(availability.WorkPostId);

        if (workPost is null)
            throw new Exception("Work Post not found.");

        await EnsureCurrentUserOwnsWorkPostAsync(workPost);

        availability.Date = dto.Date;
        availability.IsAvailable = dto.IsAvailable;
        availability.Notes = dto.Notes;

        _unitOfWork.Repository<WorkPostAvailability>()
            .Update(availability);

        await _unitOfWork.SaveChangesAsync();

        return new WorkPostAvailabilityDto
        {
            Id = availability.Id,
            WorkPostId = availability.WorkPostId,
            Date = availability.Date,
            IsAvailable = availability.IsAvailable,
            Notes = availability.Notes
        };
    }

    public async Task DeleteAsync(int id)
    {
        var availability = await _unitOfWork.Repository<WorkPostAvailability>()
            .GetByIdAsync(id);

        if (availability is null)
            throw new Exception("Availability not found.");

        var workPost = await _unitOfWork.Repository<WorkPost>()
            .GetByIdAsync(availability.WorkPostId);

        if (workPost is null)
            throw new Exception("Work Post not found.");

        await EnsureCurrentUserOwnsWorkPostAsync(workPost);

        _unitOfWork.Repository<WorkPostAvailability>()
            .Delete(availability);

        await _unitOfWork.SaveChangesAsync();
    }

    // GetByWorkPostIdAsync/IsAvailableAsync stay unauthenticated on purpose —
    // read-only calendar availability is needed while browsing a vendor's
    // page before signing in (ReserveScreen), same as WorkPostController's
    // own [AllowAnonymous] search/detail endpoints.

    public async Task<IEnumerable<WorkPostAvailabilityDto>> GetByWorkPostIdAsync(int workPostId)
    {
        var availabilities = await _unitOfWork.Repository<WorkPostAvailability>()
            .FindAsync(x => x.WorkPostId == workPostId);

        return availabilities
            .OrderBy(x => x.Date)
            .Select(x => new WorkPostAvailabilityDto
            {
                Id = x.Id,
                WorkPostId = x.WorkPostId,
                Date = x.Date,
                IsAvailable = x.IsAvailable,
                Notes = x.Notes
            });
    }

    public async Task<bool> IsAvailableAsync(int workPostId, DateOnly date)
    {
        var availability = await _unitOfWork.Repository<WorkPostAvailability>()
            .FirstOrDefaultAsync(x =>
                x.WorkPostId == workPostId &&
                x.Date == date);

        // Matches BookingService.CreateAsync's semantics — no row means the
        // date is open by default, not blocked. Only an explicit
        // IsAvailable = false row marks it unavailable.
        if (availability == null)
            return true;

        return availability.IsAvailable;
    }

    private async Task EnsureCurrentUserOwnsWorkPostAsync(WorkPost workPost)
    {
        var userIdClaim = _httpContextAccessor.HttpContext?
            .User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
            throw new UnauthorizedAccessException("User is not authenticated.");

        var vendorProfile = await _unitOfWork.Repository<VendorProfile>()
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (vendorProfile is null || workPost.VendorProfileId != vendorProfile.Id)
            throw new UnauthorizedAccessException("This work post does not belong to you.");
    }
}
