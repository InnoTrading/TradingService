using Microsoft.AspNetCore.Mvc;
using TradingService.Application.DTOs;
using TradingService.Application.Interfaces;

namespace TradingService.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController(IOrderService orderService) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> PlaceAnOrder([FromBody] PlaceOrderDto orderDto)
    {
        var result = await orderService.PlaceAnOrder(orderDto);

        if (!result)
            return BadRequest(new { error = "Failed to place order." });

        return Ok();
    }

    [HttpPatch("cancel/{id}")]
    public async Task<IActionResult> CancelAnOrder(Guid id)
    {
        var result = await orderService.CancelAnOrder(id);

        if(!result)
            return NotFound();

        return Ok();
    }
    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserOrders(string userId)
    {
        var orders = await orderService.GetUserOrders(userId);

        return Ok(orders);
    }
}