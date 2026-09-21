using System.Collections.Concurrent;
using OrderProcessing.Application.Common;
using OrderProcessing.Application.Orders.Ports;
using OrderProcessing.Domain.Orders;

namespace OrderProcessing.Api.IntegrationTests.Infrastructure;

public sealed class InMemoryOrderRepository : IOrderRepository
{
    private readonly ConcurrentDictionary<Guid, Order> _orders = new();

    public Task CreateAsync(Order order, CancellationToken cancellationToken)
    {
        _orders[order.Id] = order;
        return Task.CompletedTask;
    }

    public Task<Order?> GetByIdForUserAsync(Guid orderId, string userId, CancellationToken cancellationToken)
    {
        var order = _orders.GetValueOrDefault(orderId);
        return Task.FromResult(order?.UserId == userId ? order : null);
    }

    public Task<PagedResult<Order>> GetPagedForUserAsync(
        string userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var userOrders = _orders.Values
            .Where(order => order.UserId == userId)
            .OrderByDescending(order => order.CreatedAtUtc)
            .ToList();

        var pageItems = userOrders.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Task.FromResult(new PagedResult<Order>(pageItems, page, pageSize, userOrders.Count));
    }
}
