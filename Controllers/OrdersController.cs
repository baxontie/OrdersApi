using Microsoft.AspNetCore.Mvc;
using OrdersApi.Models;
using OrdersApi.Services;
using OrdersApi.Dtos;

namespace OrdersApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrderService _service;

    public OrdersController(OrderService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] OrderCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var order = new Order
        {
            CustomerName = dto.CustomerName,
            Status = dto.Status,
            Amount = dto.Amount,
            CreatedAt = dto.CreatedAt ?? DateTime.UtcNow
        };

        var created = await _service.CreateOrderAsync(order);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(
        int page = 1,
        int limit = 10,
        OrderStatus? status = null,
        decimal? minAmount = null,
        decimal? maxAmount = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var result = await _service.GetOrdersAsync(
            page,
            limit,
            status,
            minAmount,
            maxAmount,
            fromDate,
            toDate);

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        // Optional: Copilot can implement this too
        return NotFound();
    }
}
