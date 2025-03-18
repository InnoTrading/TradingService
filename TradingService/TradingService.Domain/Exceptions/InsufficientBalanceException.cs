namespace TradingService.Domain.Exceptions;

public class InsufficientBalanceException(decimal requiredBalance, decimal freeBalanceForOrders)
    : Exception($"Not enough cash. Required: {requiredBalance}. Available: {freeBalanceForOrders}")
{
    public decimal FreeBalanceForOrders {get; set; } = freeBalanceForOrders;
    public decimal RequiredBalance {get; set; } = requiredBalance;
}