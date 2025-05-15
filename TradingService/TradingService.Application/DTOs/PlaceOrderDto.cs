using TradingService.Domain.Entities;

namespace TradingService.Application.DTOs;

public record PlaceOrderDto(
    string UserId,
    string StockTicker,
    decimal PriceLimit,
    OperationType Operation,
    int Amount
);