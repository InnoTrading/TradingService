using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TradingService.Application.Contracts.Messaging;
using TradingService.Domain.Entities;
using TradingService.Domain.Interfaces;

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
                .Where(o => o.IsActive)
                .ToList();

            _logger.LogDebug("Found {Count} orders to process: {Orders}",
                activeOrders.Count,
                string.Join(", ", activeOrders.Select(o => $"{o.Id}"))
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
                        await ValidateOrderFundsAsync(order, currentPrice, scope.ServiceProvider, stoppingToken);

                        var orderRequest = new OrderRequest(
                            order.Id,
                            order.UserId,
                            order.StockTicker,
                            order.Operation,
                            currentPrice,
                            order.Amount,
                            DateTime.UtcNow,
                            order.PriceLimit
                        );


                        await ordersManager.ExecuteOrder(order);
                        await _tradingServiceClient.PublishOrderExecutedAsync(orderRequest, stoppingToken);

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

    private static async Task ValidateOrderFundsAsync(OrderEntity order, decimal currentPrice, IServiceProvider provider, CancellationToken ct)
    {
        var tradingServiceClient = provider.GetRequiredService<ITradingServiceClient>();

        if (order.Operation == OperationType.Sell)
        {
            var stockAmount = await tradingServiceClient.RequestUserSpecificStocksAmountForSale(order.UserId, order.StockTicker, ct);

            if (stockAmount < order.Amount)
            {
                throw new InvalidOperationException($"Insufficient stock: have {stockAmount}, need {order.Amount} of {order.StockTicker}.");
            }
        }
        else if (order.Operation == OperationType.Buy)
        {
           // To think about it
        }
        else
        {
            throw new ArgumentException($"Unknown operation type: {order.Operation}", nameof(order.Operation));
        }
    }

}
