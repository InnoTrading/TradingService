using TradingService.Domain.Entitites;

namespace TradingService.Domain.Entities;

public class OrderEntity
{
    public string StockTicker { get; set; }
    public decimal PriceLimit { get; set; }
    public OperationType Operation { get; set; }
    public int Amount { get; set; }
}