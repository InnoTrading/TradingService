using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TradingService.Domain.Entities;
using TradingService.Domain.Entitites;
using TradingService.Domain.Interfaces;

namespace TradingService.Domain.Services;

public class OrderExecutionService(
    IOrdersManager ordersManager,
    IMarketDataClient marketDataClient,
    ILogger<OrderExecutionService> logger) : BackgroundService
{
    private readonly IOrdersManager _ordersManager = ordersManager;
    private readonly IMarketDataClient _marketDataClient = marketDataClient;
    private readonly ILogger<OrderExecutionService> _logger = logger;
    private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(10);


    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OrderExecutionService started.");
        while (!stoppingToken.IsCancellationRequested)
        {
            var activeOrders = (_ordersManager as OrdersManager)?.GetActiveOrders() ??
                               Enumerable.Empty<OrderEntity>();

            foreach (var order in activeOrders)
            {
                try
                {
                    var currentPrice = await _marketDataClient.GetCurrentStockPriceAsync(order.StockTicker);
                    _logger.LogInformation(
                        "Checking order {OrderId} for ticker {Ticker}: currentPrice={CurrentPrice}, priceLimit={PriceLimit}",
                        order.Id, order.StockTicker, currentPrice, order.PriceLimit);

                    bool shouldExecute = false;

                    switch (order.Operation)
                    {
                        case OperationType.Buy:
                            if (currentPrice <= order.PriceLimit)
                                shouldExecute = true;
                            break;
                        case OperationType.Sell:
                            if (currentPrice >= order.PriceLimit)
                                shouldExecute = true;
                            break;
                    }

                    if (shouldExecute)
                    {
                        _logger.LogInformation("Executing order {OrderId}.", order.Id);
                        await _ordersManager.ExecuteOrder(order);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing order {OrderId}.", order.Id);
                }
            }

            await Task.Delay(_pollingInterval, stoppingToken);
        }
    }
}