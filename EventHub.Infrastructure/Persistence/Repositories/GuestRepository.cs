using EventHub.Application.Interfaces;
using EventHub.Domain.Entities;
using EventHub.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Infrastructure.Persistence.Repositories;

public class GuestRepository : IGuestRepository
{
    private readonly ApplicationDbContext _context;

    public GuestRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guest?> GetByIdAsync(int guestId)
    {
        return await _context.Guests.FindAsync(guestId);
    }

    public async Task<Guest?> GetByIdWithEventAsync(int guestId)
    {
        return await _context.Guests
            .Include(g => g.Event)
            .FirstOrDefaultAsync(g => g.Id == guestId);
    }

    public async Task<IEnumerable<Guest>> GetByEventIdAsync(int eventId)
    {
        return await _context.Guests
            .Where(g => g.EventId == eventId)
            .ToListAsync();
    }

    public async Task AddAsync(Guest guest)
    {
        await _context.Guests.AddAsync(guest);
    }

    public void Remove(Guest guest)
    {
        _context.Guests.Remove(guest);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}