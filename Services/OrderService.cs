using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrdersApi.Data;
using OrdersApi.Models;
using Microsoft.EntityFrameworkCore;

namespace OrdersApi.Services;

// Implement CreateOrderAsync method that saves order using EF Core
public class OrderService
{
    private readonly AppDbContext _db;

    public OrderService(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Создаёт и сохраняет заказ в базе. Если CreatedAt не задан — устанавливает UtcNow.
    /// Возвращает сохранённый экземпляр (с присвоенным Id).
    /// </summary>
    public async Task<Order> CreateOrderAsync(Order order)
    {
        if (order is null)
        {
            throw new ArgumentNullException(nameof(order));
        }

        if (order.CreatedAt == default)
        {
            order.CreatedAt = DateTime.UtcNow;
        }

        _db.Orders.Add(order);
        await _db.SaveChangesAsync().ConfigureAwait(false);

        return order;
    }

    /// <summary>
    /// Результат постраничного запроса.
    /// </summary>
    public sealed record PagedResult<T>(IEnumerable<T> Items, int TotalCount, int Page, int Limit);

    /// <summary>
    /// Возвращает список заказов с фильтрацией и пагинацией.
    /// Фильтры: status, minAmount, maxAmount, fromDate, toDate.
    /// По умолчанию: страница 1, limit 10. Limit ограничен [1..100].
    /// Результат содержит общее количество записей, элементы и параметры страницы.
    /// </summary>
    public async Task<PagedResult<Order>> GetOrdersAsync(
        int page = 1,
        int limit = 10,
        OrderStatus? status = null,
        decimal? minAmount = null,
        decimal? maxAmount = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        page = Math.Max(1, page);
        limit = Math.Clamp(limit, 1, 100);

        IQueryable<Order> query = _db.Orders.AsNoTracking();

        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        if (minAmount.HasValue)
        {
            query = query.Where(o => o.Amount >= minAmount.Value);
        }

        if (maxAmount.HasValue)
        {
            query = query.Where(o => o.Amount <= maxAmount.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(o => o.CreatedAt <= toDate.Value);
        }

        var totalCount = await query.CountAsync().ConfigureAwait(false);

        var totalPages = (int)Math.Ceiling(totalCount / (double)limit);
        var hasNextPage = page < totalPages;
        var hasPreviousPage = page > 1 && totalPages > 0;

        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync()
            .ConfigureAwait(false);

        return new PagedResult<Order>(
            items,
            totalCount,
            page,
            limit,
            totalPages,
            hasNextPage,
            hasPreviousPage
        );
    }
}