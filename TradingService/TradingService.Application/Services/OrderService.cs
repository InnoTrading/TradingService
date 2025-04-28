using AutoMapper;
using TradingService.Application.DTOs;
using TradingService.Application.Exceptions;
using TradingService.Application.Interfaces;
using TradingService.Domain.Entities;
using TradingService.Domain.Interfaces;

namespace TradingService.Application.Services;

public class OrderService(
    IOrdersManager ordersManager,
    ITradingServiceClient tradingServiceClient,
    IMapper mapper) : IOrderService
{
    private readonly IOrdersManager _ordersManager = ordersManager;
    private readonly ITradingServiceClient _tradingServiceClient = tradingServiceClient;
    private readonly IMapper _mapper = mapper;

    public async Task<bool> PlaceAnOrder(PlaceOrderDto orderDto)
    {
        var order = _mapper.Map<OrderEntity>(orderDto);

        switch (order.Operation)
        {
            case OperationType.Buy:
                var freeBalance = await _tradingServiceClient.RequestUserFreeBalanceToOrders(order.UserId);
                var cost = order.PriceLimit * order.Amount;
                if (freeBalance < cost)
                {
                    throw new InsufficientBalanceException(cost, freeBalance);
                }
                break;

            case OperationType.Sell:
                var stockAmount = await _tradingServiceClient.RequestUserSpecificStocksAmountForSale(order.UserId, order.StockTicker);
                if (stockAmount < order.Amount)
                {
                    throw new InsufficientAmountOfStocksInPortfolio(order.Amount, stockAmount);
                }
                break;

            default:
                throw new InvalidOperationException("Unknown operation type");
        }

        var result = await _ordersManager.PlaceAnOrder(order);

        return result;
    }

    public async Task<bool> CancelAnOrder(Guid orderId)
    {
        var result = await _ordersManager.CancelAnOrder(orderId);

        return result;
    }

    public async Task<OrderDto> GetOrder(Guid orderId)
    {
        var result = await _ordersManager.GetOrder(orderId);

        return _mapper.Map<OrderDto>(result);
    }

    public async Task<IEnumerable<OrderDto>> GetUserOrders(string userId)
    {
        var result = await _ordersManager.GetUserOrdersAsync(userId);

        return _mapper.Map<IEnumerable<OrderDto>>(result);
    }
}
