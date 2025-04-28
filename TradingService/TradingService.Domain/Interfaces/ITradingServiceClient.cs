using System;
using System.Threading;
using System.Threading.Tasks;

namespace TradingService.Domain.Interfaces
{
    public interface ITradingServiceClient: IAsyncDisposable, IDisposable
    {
        Task StartAsync();
        Task<decimal> RequestUserFreeBalanceToOrders(string userId, CancellationToken cancellationToken = default);
        Task PublishOrderExecutedAsync(object payload, CancellationToken cancellationToken = default);
        Task<int> RequestUserSpecificStocksAmountForSale(string userId, string stockTicker, CancellationToken cancellationToken = default);
    }
}
