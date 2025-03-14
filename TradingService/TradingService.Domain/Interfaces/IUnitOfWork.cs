using TradingService.Domain.Entities;

namespace TradingService.Domain.Interfaces;

public interface IUnitOfWork: IDisposable
{
    IRepository<OrderEntity> Orders { get; }
    Task<int> CommitAsync();
}