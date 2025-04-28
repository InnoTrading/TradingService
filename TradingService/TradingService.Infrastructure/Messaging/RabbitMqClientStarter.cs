using Microsoft.Extensions.Hosting;
using TradingService.Domain.Interfaces;

public class RabbitMqStarter : IHostedService
{
    private readonly ITradingServiceClient _tradingServiceClient;

    public RabbitMqStarter(ITradingServiceClient tradingServiceClient)
    {
        _tradingServiceClient = tradingServiceClient;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _tradingServiceClient.StartAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
