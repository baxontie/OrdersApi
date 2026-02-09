using Microsoft.AspNetCore.Mvc;
using OrdersApi.Models;
using OrdersApi.Services;

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

    // TODO: POST /api/orders
    // Accept OrderCreateDto
    // Save order
    // Return CreatedAtAction

    // TODO: GET /api/orders
    // Query params:
    //  page, limit
    //  status
    //  minAmount, maxAmount
    //  fromDate, toDate
    // Return:
    //  items + totalCount + page + limit
}
