using OrdersApi.Models;
using System.Diagnostics.CodeAnalysis;

namespace OrdersApi.Data;

[ExcludeFromCodeCoverage]
public static class SeedData
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (db.Orders.Any())
            return;

        var random = new Random();
        var statuses = Enum.GetValues<OrderStatus>();

        var orders = new List<Order>();

        for (int i = 1; i <= 50; i++)
        {
            orders.Add(new Order
            {
                CustomerName = $"Customer {i}",
                Status = statuses[random.Next(statuses.Length)],
                Amount = random.Next(10, 5000),
                CreatedAt = DateTime.UtcNow.AddDays(-random.Next(0, 90))
            });
        }

        db.Orders.AddRange(orders);
        await db.SaveChangesAsync();
    }
}
