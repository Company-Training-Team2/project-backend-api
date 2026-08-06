using EventHub.Application.Interfaces;
using EventHub.Domain.Entities;
using EventHub.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Infrastructure.Persistence.Repositories;

public class EventRepository : IEventRepository
{
    private readonly ApplicationDbContext _context;

    public EventRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Event?> GetByIdAsync(int id)
    {
        return await _context.Events
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<Event>> GetByUserIdAsync(int customerId)
    {
        return await _context.Events
            .Where(e => e.CustomerId == customerId)
            .OrderByDescending(e => e.TargetDate)
            .ToListAsync();
    }

    public async Task<Event> AddAsync(Event eventEntity)
    {
        await _context.Events.AddAsync(eventEntity);
        return eventEntity;
    }

    public async Task Remove(int id)
    {
        var entity = await _context.Events.FindAsync(id);

        if (entity != null)
        {
            _context.Events.Remove(entity);
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Events
            .AnyAsync(e => e.Id == id);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<Event?> GetByIdWithDashboardDataAsync(int id)
    {
        return await _context.Events
            .Include(e => e.Guests)
            .Include(e => e.ChecklistItems)
            .Include(e => e.Expenses)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Event?> GetByIdWithVendorsAsync(int id)
    {
        return await _context.Events
            .Include(e => e.Bookings)
                .ThenInclude(b => b.WorkPost)
                    .ThenInclude(w => w.VendorProfile)
            .FirstOrDefaultAsync(e => e.Id == id);
    }
}
