using System.Text.Json;
using TradingService.Domain.Interfaces;

namespace TradingService.Domain.Managers;

public class MarketDataClient(HttpClient httpClient): IMarketDataClient
{
    public async Task<decimal> GetCurrentStockPriceAsync(string ticker)
    {
        var response = await httpClient.GetAsync($"https://localhost:7165/api/marketData/price/{ticker}");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var price = JsonSerializer.Deserialize<decimal>(json);
        return price;
    }
}