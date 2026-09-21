using Dapper;
using OrderProcessing.Application.Processing.Ports;
using OrderProcessing.Domain.Orders;
using OrderProcessing.Domain.Processing;

namespace OrderProcessing.Infrastructure.Persistence;

internal sealed class OrderProcessingRepository : IOrderProcessingRepository
{
    private readonly INpgsqlConnectionFactory _connectionFactory;

    public OrderProcessingRepository(INpgsqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<OrderClaim?> ClaimNextPendingOrderAsync(CancellationToken cancellationToken)
    {
        const string claimSql = """
            WITH next_order AS (
                SELECT id
                FROM orders
                WHERE status = 'Pending'
                ORDER BY created_at_utc
                FOR UPDATE SKIP LOCKED
                LIMIT 1
            )
            UPDATE orders
            SET status = 'Processing', updated_at_utc = now()
            FROM next_order
            WHERE orders.id = next_order.id
            RETURNING orders.id, orders.user_id, orders.created_at_utc, orders.updated_at_utc
            """;

        const string itemsSql = """
            SELECT id, order_id, product_id, product_name, unit_price, quantity
            FROM order_items
            WHERE order_id = @OrderId
            """;

        const string nextAttemptNumberSql = """
            SELECT COALESCE(MAX(attempt_number), 0) + 1
            FROM order_processing_attempts
            WHERE order_id = @OrderId
            """;

        using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        var claimedOrderRow = await connection.QuerySingleOrDefaultAsync<ClaimedOrderRow>(
            new CommandDefinition(claimSql, transaction: transaction, cancellationToken: cancellationToken));

        if (claimedOrderRow is null)
        {
            await transaction.CommitAsync(cancellationToken);
            return null;
        }

        var itemRows = await connection.QueryAsync<OrderItemRow>(new CommandDefinition(
            itemsSql,
            new { OrderId = claimedOrderRow.Id },
            transaction,
            cancellationToken: cancellationToken));

        var attemptNumber = await connection.ExecuteScalarAsync<int>(new CommandDefinition(
            nextAttemptNumberSql,
            new { OrderId = claimedOrderRow.Id },
            transaction,
            cancellationToken: cancellationToken));

        await transaction.CommitAsync(cancellationToken);

        var items = itemRows
            .Select(item => OrderItem.Restore(item.Id, item.ProductId, item.ProductName, item.UnitPrice, item.Quantity))
            .ToList();

        var order = Order.Restore(
            claimedOrderRow.Id,
            claimedOrderRow.UserId,
            OrderStatus.Processing,
            claimedOrderRow.CreatedAtUtc,
            claimedOrderRow.UpdatedAtUtc,
            items);

        return new OrderClaim(order, attemptNumber);
    }

    public async Task FinalizeAttemptAsync(OrderProcessingAttempt attempt, bool orderSucceeded, CancellationToken cancellationToken)
    {
        const string insertAttemptSql = """
            INSERT INTO order_processing_attempts
                (id, order_id, attempt_number, started_at_utc, finished_at_utc, success, error_message)
            VALUES
                (@Id, @OrderId, @AttemptNumber, @StartedAtUtc, @FinishedAtUtc, @Success, @ErrorMessage)
            """;

        const string updateOrderStatusSql = """
            UPDATE orders
            SET status = @Status, updated_at_utc = now()
            WHERE id = @OrderId
            """;

        using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await connection.ExecuteAsync(new CommandDefinition(
            insertAttemptSql,
            new
            {
                attempt.Id,
                attempt.OrderId,
                attempt.AttemptNumber,
                attempt.StartedAtUtc,
                attempt.FinishedAtUtc,
                attempt.Success,
                attempt.ErrorMessage
            },
            transaction,
            cancellationToken: cancellationToken));

        var status = orderSucceeded ? OrderStatus.Completed : OrderStatus.Failed;

        await connection.ExecuteAsync(new CommandDefinition(
            updateOrderStatusSql,
            new { OrderId = attempt.OrderId, Status = status.ToString() },
            transaction,
            cancellationToken: cancellationToken));

        await transaction.CommitAsync(cancellationToken);
    }

    private sealed record ClaimedOrderRow(Guid Id, string UserId, DateTime CreatedAtUtc, DateTime UpdatedAtUtc);

    private sealed record OrderItemRow(Guid Id, Guid OrderId, Guid ProductId, string ProductName, decimal UnitPrice, int Quantity);
}
