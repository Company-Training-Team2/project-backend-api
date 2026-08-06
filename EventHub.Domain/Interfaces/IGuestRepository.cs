using EventHub.Domain.Entities;

namespace EventHub.Application.Interfaces;

public interface IGuestRepository
{
    Task<Guest?> GetByIdAsync(int guestId);
    Task<Guest?> GetByIdWithEventAsync(int guestId);
    Task<IEnumerable<Guest>> GetByEventIdAsync(int eventId);

    Task AddAsync(Guest guest);
    void Remove(Guest guest);
    Task<int> SaveChangesAsync();
}