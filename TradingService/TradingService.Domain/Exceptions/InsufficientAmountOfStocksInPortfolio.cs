using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradingService.Domain.Exceptions
{
    internal class InsufficientAmountOfStocksInPortfolio(int requiredAmount, int amount) : 
        Exception($"Insufficient amount of stocks in your portfolio. Required: {requiredAmount}. Avaible: {amount}")
    {
        public int RequiredAmount { get; set; } = requiredAmount;
        public int Amount { get; set; } = amount;
    }
}
