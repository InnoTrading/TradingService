using TradingService.Application.DTOs;

namespace TradingService.Application.Interfaces;

public interface IOrderService
{
    Task<bool> PlaceAnOrder(PlaceOrderDto order);
    Task<bool> CancelAnOrder(Guid orderId);
    Task<OrderDto> GetOrder(Guid orderId);
    Task<IEnumerable<OrderDto>> GetUserOrders(string userId);
}