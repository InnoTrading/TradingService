using Microsoft.Extensions.DependencyInjection;
using TradingService.Domain.Interfaces;
using TradingService.Domain.Services;

namespace TradingService.Domain.Extensions
{
    public static class DomainExtensions
    {
        public static IServiceCollection AddDomainServices(this IServiceCollection services)
        {
            services.AddScoped<IOrdersManager, OrdersManager>();
            services.AddSingleton<IActiveOrdersStore, ActiveOrdersStore>();

            services.AddHostedService<OrdersInitializationService>();
            services.AddHostedService<OrderExecutionService>();

            services.AddHttpClient<IMarketDataClient, MarketDataClient>();

            return services;
        }
    }
}
