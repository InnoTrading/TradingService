using TradingService.Domain.Entities;

namespace TradingService.Domain.Interfaces;

public interface IOrdersManager
{
    Task<bool> PlaceAnOrder(OrderEntity order);
    Task<bool> CancelAnOrder(Guid id);
    Task<OrderEntity?> GetOrder(Guid id);
    Task<IEnumerable<OrderEntity>> GetUserOrdersAsync(string userId);
    Task<IEnumerable<OrderEntity>> GetActiveOrdersAsync();
   Task ExecuteOrder(OrderEntity order);
    Task InitializeOrdersAsync();
}