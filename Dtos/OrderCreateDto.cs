using OrdersApi.Models;
using System.ComponentModel.DataAnnotations;

namespace OrdersApi.Dtos;

public class OrderCreateDto
{
    [Required]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    public OrderStatus Status { get; set; }

    [Range(0.01, 1_000_000)]
    public decimal Amount { get; set; }

    public DateTime? CreatedAt { get; set; }
}
