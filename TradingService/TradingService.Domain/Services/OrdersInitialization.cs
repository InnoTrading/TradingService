using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TradingService.Domain.Interfaces;

namespace TradingService.Domain.Services;

public class OrdersInitializationService(IServiceProvider serviceProvider) : IHostedService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;


    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var ordersManager = scope.ServiceProvider.GetService<IOrdersManager>();
        if (ordersManager == null)
            throw new InvalidOperationException();

        await ordersManager.InitializeOrdersAsync();
    }
        
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}