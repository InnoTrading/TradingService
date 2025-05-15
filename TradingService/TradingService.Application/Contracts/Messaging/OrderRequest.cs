using TradingService.Domain.Entities;

namespace TradingService.Application.Contracts.Messaging
{
    public record OrderRequest(
        Guid orderId,
        string userId,
        string ticker,
        OperationType operation,
        decimal executedPrice,
        int amount,
        DateTime executedAt,
        decimal PriceLimit
    );

}
