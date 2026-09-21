using Dapper;
using OrderProcessing.Application.Common;
using OrderProcessing.Application.Orders.Ports;
using OrderProcessing.Domain.Orders;

namespace OrderProcessing.Infrastructure.Persistence;

internal sealed class OrderRepository : IOrderRepository
{
    private readonly INpgsqlConnectionFactory _connectionFactory;

    public OrderRepository(INpgsqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task CreateAsync(Order order, CancellationToken cancellationToken)
    {
        const string insertOrderSql = """
            INSERT INTO orders (id, user_id, status, created_at_utc, updated_at_utc)
            VALUES (@Id, @UserId, @Status, @CreatedAtUtc, @UpdatedAtUtc)
            """;

        const string insertItemSql = """
            INSERT INTO order_items (id, order_id, product_id, product_name, unit_price, quantity)
            VALUES (@Id, @OrderId, @ProductId, @ProductName, @UnitPrice, @Quantity)
            """;

        using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await connection.ExecuteAsync(new CommandDefinition(
            insertOrderSql,
            new
            {
                order.Id,
                order.UserId,
                Status = order.Status.ToString(),
                order.CreatedAtUtc,
                order.UpdatedAtUtc
            },
            transaction,
            cancellationToken: cancellationToken));

        foreach (var item in order.Items)
        {
            await connection.ExecuteAsync(new CommandDefinition(
                insertItemSql,
                new
                {
                    item.Id,
                    OrderId = order.Id,
                    item.ProductId,
                    item.ProductName,
                    item.UnitPrice,
                    item.Quantity
                },
                transaction,
                cancellationToken: cancellationToken));
        }

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<Order?> GetByIdForUserAsync(Guid orderId, string userId, CancellationToken cancellationToken)
    {
        const string orderSql = """
            SELECT id, user_id, status, created_at_utc, updated_at_utc
            FROM orders
            WHERE id = @OrderId AND user_id = @UserId
            """;

        const string itemsSql = """
            SELECT id, order_id, product_id, product_name, unit_price, quantity
            FROM order_items
            WHERE order_id = @OrderId
            """;

        using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

        var orderRow = await connection.QuerySingleOrDefaultAsync<OrderRow>(
            new CommandDefinition(orderSql, new { OrderId = orderId, UserId = userId }, cancellationToken: cancellationToken));

        if (orderRow is null)
        {
            return null;
        }

        var itemRows = await connection.QueryAsync<OrderItemRow>(
            new CommandDefinition(itemsSql, new { OrderId = orderId }, cancellationToken: cancellationToken));

        return ToDomain(orderRow, itemRows);
    }

    public async Task<PagedResult<Order>> GetPagedForUserAsync(
        string userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        const string countSql = "SELECT COUNT(*) FROM orders WHERE user_id = @UserId";

        const string pageSql = """
            SELECT id, user_id, status, created_at_utc, updated_at_utc
            FROM orders
            WHERE user_id = @UserId
            ORDER BY created_at_utc DESC
            OFFSET @Offset LIMIT @PageSize
            """;

        const string itemsSql = """
            SELECT id, order_id, product_id, product_name, unit_price, quantity
            FROM order_items
            WHERE order_id = ANY(@OrderIds)
            """;

        using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

        var totalCount = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(countSql, new { UserId = userId }, cancellationToken: cancellationToken));

        var orderRows = (await connection.QueryAsync<OrderRow>(new CommandDefinition(
            pageSql,
            new { UserId = userId, Offset = (page - 1) * pageSize, PageSize = pageSize },
            cancellationToken: cancellationToken))).ToList();

        var orderIds = orderRows.Select(row => row.Id).ToArray();
        var itemRows = orderIds.Length == 0
            ? Enumerable.Empty<OrderItemRow>()
            : await connection.QueryAsync<OrderItemRow>(
                new CommandDefinition(itemsSql, new { OrderIds = orderIds }, cancellationToken: cancellationToken));

        var itemsByOrderId = itemRows.GroupBy(item => item.OrderId).ToDictionary(group => group.Key, group => group.ToList());

        var orders = orderRows
            .Select(orderRow => ToDomain(orderRow, itemsByOrderId.GetValueOrDefault(orderRow.Id, new List<OrderItemRow>())))
            .ToList();

        return new PagedResult<Order>(orders, page, pageSize, totalCount);
    }

    private static Order ToDomain(OrderRow orderRow, IEnumerable<OrderItemRow> itemRows)
    {
        var items = itemRows
            .Select(item => OrderItem.Restore(item.Id, item.ProductId, item.ProductName, item.UnitPrice, item.Quantity))
            .ToList();

        return Order.Restore(
            orderRow.Id,
            orderRow.UserId,
            Enum.Parse<OrderStatus>(orderRow.Status),
            orderRow.CreatedAtUtc,
            orderRow.UpdatedAtUtc,
            items);
    }

    private sealed record OrderRow(Guid Id, string UserId, string Status, DateTime CreatedAtUtc, DateTime UpdatedAtUtc);

    private sealed record OrderItemRow(Guid Id, Guid OrderId, Guid ProductId, string ProductName, decimal UnitPrice, int Quantity);
}
