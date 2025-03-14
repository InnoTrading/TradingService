using System.Collections.Concurrent;
using TradingService.Domain.Entities;

namespace TradingService.Domain.Interfaces;

public interface IActiveOrdersStore
{
    ConcurrentDictionary<Guid, OrderEntity> ActiveOrders { get; }
}