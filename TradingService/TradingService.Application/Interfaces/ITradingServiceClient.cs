using TradingService.Application.Contracts.Messaging;

namespace TradingService.Domain.Interfaces
{
    public interface ITradingServiceClient: IAsyncDisposable, IDisposable
    {
        Task StartAsync();
        Task<decimal> RequestUserAvaibleBalanceToOrders(string userId, CancellationToken cancellationToken = default);
        Task<int> RequestUserSpecificStocksAmountForSale(string userId, string stockTicker, CancellationToken cancellationToken = default);
        Task PublishOrderExecutedAsync(OrderRequest payload, CancellationToken cancellationToken = default);
        Task<bool> ReserveBalance(string userId, decimal amount, CancellationToken ct = default);
        Task<bool> ReleaseReservedBalance(string userId, decimal amount, CancellationToken ct = default);

    }
}
