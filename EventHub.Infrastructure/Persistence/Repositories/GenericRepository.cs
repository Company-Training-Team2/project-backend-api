using System.Linq.Expressions;
using EventHub.Domain.Interfaces;
using EventHub.Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace EventHub.Infrastructure.Persistence.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : class
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(int id) =>
        await _dbSet.FindAsync(id);

    public async Task<IEnumerable<T>> GetAllAsync() =>
        await _dbSet.ToListAsync();

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) =>
        await _dbSet.Where(predicate).ToListAsync();

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate) =>
        await _dbSet.FirstOrDefaultAsync(predicate);

    public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate) =>
        await _dbSet.AnyAsync(predicate);

    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null) =>
        predicate is null
            ? await _dbSet.CountAsync()
            : await _dbSet.CountAsync(predicate);

    public async Task AddAsync(T entity) =>
        await _dbSet.AddAsync(entity);

    public async Task AddRangeAsync(IEnumerable<T> entities) =>
        await _dbSet.AddRangeAsync(entities);

    public void Update(T entity) =>
        _dbSet.Update(entity);

    public void Delete(T entity) =>
        _dbSet.Remove(entity);

    public void DeleteRange(IEnumerable<T> entities) =>
        _dbSet.RemoveRange(entities);

    public async Task<bool> ExistsAsync(int id) =>
        await _dbSet.FindAsync(id) is not null;

    public async Task<IEnumerable<T>> GetPagedAsync(int pageNumber, int pageSize) =>
        await _dbSet
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
}
