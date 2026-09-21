using OrderProcessing.Application.Common;
using OrderProcessing.Domain.Orders;

namespace OrderProcessing.Application.Orders.Ports;

public interface IOrderRepository
{
    Task CreateAsync(Order order, CancellationToken cancellationToken);

    Task<Order?> GetByIdForUserAsync(Guid orderId, string userId, CancellationToken cancellationToken);

    Task<PagedResult<Order>> GetPagedForUserAsync(string userId, int page, int pageSize, CancellationToken cancellationToken);
}
