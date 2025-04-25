using System.Collections.Concurrent;
using System.Text;
using TradingService.Domain.Entities;
using TradingService.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using TradingService.Domain.Exceptions;
using System.Net.Sockets;

namespace TradingService.Domain.Services;

public class OrdersManager(
    IUnitOfWork unitOfWork,
    ILogger<OrdersManager> logger,
    IActiveOrdersStore activeOrdersStore,
    ITradingServiceClient tradingServiceClient) : IOrdersManager
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly ILogger<OrdersManager> _logger = logger;
    private readonly IActiveOrdersStore _activeOrdersStore = activeOrdersStore;
    private readonly ITradingServiceClient _tradingServiceClient = tradingServiceClient;

    public async Task InitializeOrdersAsync()
    {
        var orders = await _unitOfWork.Orders.GetByConditionAsync(o => o.IsActive);
        foreach (var order in orders)
        {
            _activeOrdersStore.ActiveOrders[order.Id] = order;
        }

        _logger.LogInformation("Initialized active orders structure with {Count} orders.",
            _activeOrdersStore.ActiveOrders.Count);
    }

    public async Task<bool> PlaceAnOrder(OrderEntity order)
    {
        await _tradingServiceClient.StartAsync();
        decimal freeBalance = await _tradingServiceClient.RequestUserFreeBalanceToOrders(order.UserId);

        if (freeBalance < order.PriceLimit * order.Amount)
            throw new InsufficientBalanceException(order.PriceLimit * order.Amount, freeBalance);

        var result = await _unitOfWork.Orders.AddAsync(order);
        if (result)
        {
            _activeOrdersStore.ActiveOrders[order.Id] = order;
            _logger.LogInformation("Order {OrderId} placed and added to active orders.", order.Id);
        }

        await _unitOfWork.CommitAsync();
        return result;
    }

    public async Task<bool> CancelAnOrder(Guid id)
    {
        if (!_activeOrdersStore.ActiveOrders.ContainsKey(id))
            return false;

        var order = await _unitOfWork.Orders.GetByIdAsync(id);        

        if (order == null || order.IsCompleted)
            return false;

        order.IsActive = false;
        _logger.LogInformation("Order {OrderId} cancelled, removed from active orders.", id);
        if(_activeOrdersStore.ActiveOrders.TryRemove(id, out var deleted))
        {
            await _unitOfWork.CommitAsync();
        }
        return true;
    }

    public async Task<OrderEntity?> GetOrder(Guid id)
    {
        if (_activeOrdersStore.ActiveOrders.TryGetValue(id, out var order))
            return order;
        return await _unitOfWork.Orders.GetByIdAsync(id);
    }

    public Task<IEnumerable<OrderEntity>> GetUserOrdersAsync(string userId)
    {
        var result = _activeOrdersStore.ActiveOrders.Values
                        .Where(o => o.UserId == userId);
        return Task.FromResult(result);
    }

    public async Task ExecuteOrder(OrderEntity order)
    {
        order.IsCompleted = true;
        order.IsActive = false;
        _activeOrdersStore.ActiveOrders.TryRemove(order.Id, out _);
        await _unitOfWork.Orders.UpdateAsync(order);
        await _unitOfWork.CommitAsync();
        _logger.LogInformation("Order {OrderId} executed and removed from active orders.", order.Id);
    }

    public Task<IEnumerable<OrderEntity>> GetActiveOrdersAsync()
     => Task.FromResult(_activeOrdersStore.ActiveOrders.Values.AsEnumerable());

}