namespace OrdersApi.Models;

public enum OrderStatus
{
    Pending,
    Paid,
    Shipped,
    Cancelled
}

public class Order
{
    public int Id { get; set; }

    public string CustomerName { get; set; } = string.Empty;

    public OrderStatus Status { get; set; }

    public decimal Amount { get; set; }

    public DateTime CreatedAt { get; set; }
}
