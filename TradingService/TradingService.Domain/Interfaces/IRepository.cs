using System.Linq.Expressions;

namespace TradingService.Domain.Interfaces;

public interface IRepository<T> where T : class
{
    public Task<T?> GetByIdAsync(Guid id);
    public Task<IEnumerable<T>> GetByConditionAsync(Expression<Func<T, bool>> predicate);
    
    public Task<bool> AddAsync(T entity);
    public Task<bool> RemoveAsync(T entity);
    public Task<bool> UpdateAsync(T entity);
}