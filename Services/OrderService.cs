using OrdersApi.Data;
using OrdersApi.Models;
using Microsoft.EntityFrameworkCore;

namespace OrdersApi.Services;

// Implement CreateOrderAsync method that saves order using EF Core

// - CreateOrderAsync(Order order)
// - GetOrdersAsync with pagination:
//      page, limit
// - Filtering by:
//      status
//      minAmount / maxAmount
//      fromDate / toDate
// - Return total count + items
public class OrderService
{
    private readonly AppDbContext _db;

    public OrderService(AppDbContext db)
    {
        _db = db;
    }
}
