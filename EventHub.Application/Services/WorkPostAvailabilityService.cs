using EventHub.Application.DTOs.WorkPostAvailability;
using EventHub.Application.Interfaces;
using EventHub.Domain.Entities;
using EventHub.Domain.Interfaces;

namespace EventHub.Application.Services;

public class WorkPostAvailabilityService : IWorkPostAvailabilityService
{
    private readonly IUnitOfWork _unitOfWork;

    public WorkPostAvailabilityService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<WorkPostAvailabilityDto> CreateAsync(CreateWorkPostAvailabilityDto dto)
    {
        var workPost = await _unitOfWork.Repository<WorkPost>()
            .GetByIdAsync(dto.WorkPostId);

        if (workPost is null)
            throw new Exception("Work Post not found.");

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

        _unitOfWork.Repository<WorkPostAvailability>()
            .Delete(availability);

        await _unitOfWork.SaveChangesAsync();
    }

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

        if (availability == null)
            return false;

        return availability.IsAvailable;
    }
}