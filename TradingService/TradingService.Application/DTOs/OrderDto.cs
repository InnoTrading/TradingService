using TradingService.Domain.Entitites;

namespace TradingService.Application.DTOs;

public record OrderDto(
    Guid Id,
    string StockTicker,
    decimal PriceLimit,
    OperationType Operation,
    int Amount,
    bool IsCompleted,
    bool IsActive
);