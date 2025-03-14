using TradingService.Domain.Entities;
using TradingService.Domain.Interfaces;
using TradingService.Infrastructure.Data;

namespace TradingService.Infrastructure.Repositories;

public class UnitOfWork(TradingDbContext context) : IUnitOfWork
{
    private IRepository<OrderEntity>? _orders;

    public IRepository<OrderEntity> Orders => _orders ??= new Repository<OrderEntity>(context);

    public async Task<int> CommitAsync()
    {
        return await context.SaveChangesAsync();
    }

    public void Dispose()
    {
        context.Dispose();
    }
}