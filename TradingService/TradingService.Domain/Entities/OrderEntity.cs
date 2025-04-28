namespace TradingService.Domain.Entities;

public class OrderEntity : BaseEntity
{
    public string UserId { get; set; }
    public string StockTicker { get; set; }
    public decimal PriceLimit { get; set; }
    public OperationType Operation { get; set; }
    public int Amount { get; set; }
    public bool IsCompleted { get; set; } = false;
    public bool IsActive { get; set; } = true;

}