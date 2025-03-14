using AutoMapper;
using TradingService.Application.DTOs;
using TradingService.Domain.Entities;

namespace TradingService.Application.Mapping;

public class PlaceOrderMappingProfile: Profile
{
    public PlaceOrderMappingProfile()
    {
        CreateMap<PlaceOrderDto, OrderEntity>();
    }
}