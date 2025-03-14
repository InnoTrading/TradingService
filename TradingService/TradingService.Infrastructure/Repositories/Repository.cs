using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TradingService.Domain.Entities;
using TradingService.Domain.Interfaces;
using TradingService.Infrastructure.Data;

namespace TradingService.Infrastructure.Repositories;

public class Repository<T>(TradingDbContext context) : IRepository<T> where T : BaseEntity
{
    protected readonly DbSet<T> DbSet = context.Set<T>();
    private readonly TradingDbContext _context = context;

    public async Task<bool> AddAsync(T entity)
    {
        await DbSet.AddAsync(entity);

        var idProperty = typeof(T).GetProperty("Id");
        if (idProperty == null)
        {
            return false;
        }

        return true;
    }

    public async Task<bool> RemoveAsync(T entity)
    {
        DbSet.Remove(entity);
        return await Task.FromResult(true);
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await DbSet.FindAsync(id);
    }

    public async Task<IEnumerable<T>> GetByConditionAsync(Expression<Func<T, bool>> predicate)
    {
        return await DbSet.Where(predicate).ToListAsync();
    }

    public async Task<bool> UpdateAsync(T entity)
    {
        var entry = _context.Entry(entity);
        if (entry.State == EntityState.Detached)
        {
            DbSet.Attach(entity);
        } 
        entry.State = EntityState.Modified;

        return await Task.FromResult(true);
    }
}