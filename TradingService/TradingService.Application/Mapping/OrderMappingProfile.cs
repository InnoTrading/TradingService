using AutoMapper;
using TradingService.Application.DTOs;
using TradingService.Domain.Entities;

namespace TradingService.Application.Mapping;

public class OrderMappingProfile : Profile
{
    public OrderMappingProfile()
    {
        CreateMap<OrderEntity, OrderDto>();
    }
}