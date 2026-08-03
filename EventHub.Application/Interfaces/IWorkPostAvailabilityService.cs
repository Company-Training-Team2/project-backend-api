using EventHub.Application.DTOs.WorkPostAvailability;

namespace EventHub.Application.Interfaces;

public interface IWorkPostAvailabilityService
{
    Task<WorkPostAvailabilityDto> CreateAsync(CreateWorkPostAvailabilityDto dto);

    Task<WorkPostAvailabilityDto> UpdateAsync(int id, UpdateWorkPostAvailabilityDto dto);

    Task DeleteAsync(int id);

    Task<IEnumerable<WorkPostAvailabilityDto>> GetByWorkPostIdAsync(int workPostId);

    Task<bool> IsAvailableAsync(int workPostId, DateOnly date);
}