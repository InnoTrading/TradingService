using TradingService.Application.Contracts.Messaging;

namespace TradingService.Domain.Interfaces
{
    public interface ITradingServiceClient: IAsyncDisposable, IDisposable
    {
        Task StartAsync();
        Task<decimal> RequestUserFreeBalanceToOrders(string userId, CancellationToken cancellationToken = default);
        Task<int> RequestUserSpecificStocksAmountForSale(string userId, string stockTicker, CancellationToken cancellationToken = default);
        Task PublishOrderExecutedAsync(OrderRequest payload, CancellationToken cancellationToken = default);
    }
}
