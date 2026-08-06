using EventHub.Domain.Entities;

namespace EventHub.Application.Interfaces
{
    public interface IEventRepository
    {
        Task<Event?> GetByIdAsync(int id);
        Task<IEnumerable<Event>> GetByUserIdAsync(int customerId);
        Task<Event> AddAsync(Event eventEntity);
        Task Remove(int id);
        Task<bool> ExistsAsync(int id);
        Task SaveChangesAsync();

        /// <summary>Loads Guests, ChecklistItems and Expenses for the dashboard aggregation.</summary>
        Task<Event?> GetByIdWithDashboardDataAsync(int id);

        /// <summary>Loads Bookings with their WorkPost and VendorProfile for the vendors list.</summary>
        Task<Event?> GetByIdWithVendorsAsync(int id);
    }
}
