namespace TradingService.Domain.Interfaces;

public interface IMarketDataClient
{
    Task<decimal> GetCurrentStockPriceAsync(string stockTicker);
}