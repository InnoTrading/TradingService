using System;

namespace TradingService.Infrastructure.Messaging.Dtos
{
    public record OrderRequest(
        string UserId,
        string StockTicker,
        int Quantity,
        decimal PricePerShare,
        int Operation
    );
}
