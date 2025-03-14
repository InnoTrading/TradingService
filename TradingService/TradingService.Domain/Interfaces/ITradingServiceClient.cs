namespace TradingService.Domain.Interfaces;

public interface ITradingServiceClient
{
    Task<decimal> RequestUserFreeBalanceToOrders(string userId, CancellationToken cancellationToken = default);
    Task StartAsync();
}