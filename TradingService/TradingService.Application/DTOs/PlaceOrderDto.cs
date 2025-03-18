using TradingService.Domain.Entitites;

namespace TradingService.Application.DTOs;

public record PlaceOrderDto(
    string UserId,
    string StockTicker,
    decimal PriceLimit,
    OperationType Operation,
    int Amount
);