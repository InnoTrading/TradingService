using Microsoft.Extensions.DependencyInjection;
using TradingService.Application.Interfaces;
using TradingService.Application.Mapping;
using TradingService.Application.Services;

namespace TradingService.Application.Extensions;

public static class ApplicationExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IOrderService, OrderService>();
        services.AddAutoMapper(typeof(OrderMappingProfile));
        services.AddAutoMapper(typeof(PlaceOrderMappingProfile));
        return services;
    }
}