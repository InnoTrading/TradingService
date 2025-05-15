using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using TradingService.Domain.Interfaces;

namespace TradingService.Domain.Services
{
    public class MarketDataClient : IMarketDataClient
    {
        private readonly HttpClient _httpClient;

        public MarketDataClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<decimal> GetCurrentStockPriceAsync(string ticker, CancellationToken cancellationToken = default)
        {
            // Przygotuj żądanie SSE
            using var request = new HttpRequestMessage(HttpMethod.Get,
                $"https://localhost:5012/gateway/market-data/price/{ticker}");
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

            using var response = await _httpClient
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = new StreamReader(stream);

            string? line;
            while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
            {
                line = await reader.ReadLineAsync();
                if (line is null)
                    continue;

                if (line.StartsWith("data:"))
                {
                    var json = line["data:".Length..].Trim();
                    using var doc = JsonDocument.Parse(json);
                    if (doc.RootElement.TryGetProperty("price", out var priceEl)
                        && priceEl.TryGetDecimal(out var price))
                    {
                        return price;
                    }
                }
            }

            throw new InvalidOperationException("Nie otrzymano danych cenowych z SSE.");
        }
    }
}