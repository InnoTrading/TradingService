using System.Collections.Concurrent;
using TradingService.Domain.Entities;
using TradingService.Domain.Interfaces;

namespace TradingService.Domain.Services;

public class ActiveOrdersStore : IActiveOrdersStore
{
    public ConcurrentDictionary<Guid, OrderEntity> ActiveOrders { get; } = new();
}