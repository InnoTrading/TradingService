namespace TradingService.Domain.Interfaces;

public interface ITradingServiceClient
{
    Task<decimal> RequestUserFreeBalanceToOrders(string userId, CancellationToken cancellationToken = default);
    Task StartAsync();
    Task PublishOrderExecutedAsync(object payload, CancellationToken ct = default);

}