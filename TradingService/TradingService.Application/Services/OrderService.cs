using AutoMapper;
using TradingService.Application.DTOs;
using TradingService.Application.Interfaces;
using TradingService.Domain.Entities;
using TradingService.Domain.Interfaces;

namespace TradingService.Application.Services;

public class OrderService(IOrdersManager ordersManager, IMapper mapper) : IOrderService
{
    public async Task<bool> PlaceAnOrder(PlaceOrderDto order)
    {
        var result = await ordersManager.PlaceAnOrder(mapper.Map<OrderEntity>(order));

        return result;
    }

    public async Task<bool> CancelAnOrder(Guid orderId)
    {
        var result = await ordersManager.CancelAnOrder(orderId);

        return result;
    }

    public async Task<OrderDto> GetOrder(Guid orderId)
    {
        var result = await ordersManager.GetOrder(orderId);

        return mapper.Map<OrderDto>(result);
    }

    public async Task<IEnumerable<OrderDto>> GetUserOrders(string userId)
    {
        var result = await ordersManager.GetUserOrders(userId);

        return mapper.Map<IEnumerable<OrderDto>>(result);
    }
}