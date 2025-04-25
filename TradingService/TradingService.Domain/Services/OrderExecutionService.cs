using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TradingService.Domain.Entities;
using TradingService.Domain.Entitites;
using TradingService.Domain.Interfaces;

namespace TradingService.Domain.Services
{
    public class OrderExecutionService(
        IServiceProvider serviceProvider,
        IMarketDataClient marketDataClient,
        ITradingServiceClient tradingServiceClient,
        ILogger<OrderExecutionService> logger) : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly IMarketDataClient _marketDataClient = marketDataClient;
        private readonly ITradingServiceClient _tradingServiceClient = tradingServiceClient;
        private readonly ILogger<OrderExecutionService> _logger = logger;

        private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(1);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("OrderExecutionService is starting.");


            using (var initScope = _serviceProvider.CreateScope())
            {
                var initMgr = initScope.ServiceProvider.GetRequiredService<IOrdersManager>();
                await initMgr.InitializeOrdersAsync();
                _logger.LogInformation("Initialized orders cache at startup.");
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Polling active orders at {Time}", DateTime.UtcNow);

                using var scope = _serviceProvider.CreateScope();
                var ordersManager = scope.ServiceProvider.GetRequiredService<IOrdersManager>();

                var activeOrders = (await ordersManager.GetActiveOrdersAsync())
                                       .Where(o => o.IsActive == true)
                                       .ToList();

                // debug: ile i jakie statusy
                _logger.LogDebug("Found {Count} orders to process: {Orders}",
                    activeOrders.Count,
                    string.Join(", ", activeOrders.Select(o => $"{o.Id} - active: {o.IsActive}"))
                );

                foreach (var order in activeOrders)
                {
                    _logger.LogInformation("Processing order {OrderId}", order.Id);
                    try
                    {
                        var currentPrice = await _marketDataClient
                            .GetCurrentStockPriceAsync(order.StockTicker, stoppingToken);

                        bool shouldExecute = order.Operation switch
                        {
                            OperationType.Buy when currentPrice <= order.PriceLimit => true,
                            OperationType.Sell when currentPrice >= order.PriceLimit => true,
                            _ => false
                        };

                        if (shouldExecute)
                        {
                            await ordersManager.ExecuteOrder(order);
                            await _tradingServiceClient.PublishOrderExecutedAsync(new
                            {
                                orderId = order.Id,
                                userId = order.UserId,
                                ticker = order.StockTicker,
                                operation = order.Operation.ToString(),
                                executedPrice = currentPrice,
                                amount = order.Amount,
                                executedAt = DateTime.UtcNow
                            });
                            _logger.LogInformation("Executed order {OrderId} at {Price}", order.Id, currentPrice);
                        }
                        else
                        {
                            _logger.LogInformation(
                                "Skipping order {OrderId}: price {Price} does not meet limit {Limit}",
                                order.Id, currentPrice, order.PriceLimit);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error processing order {OrderId}", order.Id);
                    }
                }

                await Task.Delay(PollingInterval, stoppingToken);
            }
        }
    }
}
