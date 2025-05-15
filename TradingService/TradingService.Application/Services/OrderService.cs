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
                var availableBalance = await _tradingServiceClient.RequestUserAvaibleBalanceToOrders(order.UserId);
                var cost = order.PriceLimit * order.Amount;
                if (availableBalance < cost)
                {
                    throw new InsufficientBalanceException(cost, availableBalance);
                }

                var reserveSuccess = await _tradingServiceClient.ReserveBalance(order.UserId, cost);
                if (!reserveSuccess)
                {
                    throw new InvalidOperationException("Failed to reserve balance for the order.");
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
        var order = await _ordersManager.GetOrder(orderId);

        if (order == null)
            return false;

        if (!order.IsActive)
            return false;

        var cancelResult = await _ordersManager.CancelAnOrder(orderId);

        if (cancelResult && order.Operation == OperationType.Buy)
        {
            var cost = order.PriceLimit * order.Amount;
            var releaseSuccess = await _tradingServiceClient.ReleaseReservedBalance(order.UserId, cost);
            if (!releaseSuccess)
            {
                throw new InvalidOperationException("Failed to release reserved balance after order cancellation.");
            }
        }

        return cancelResult;
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