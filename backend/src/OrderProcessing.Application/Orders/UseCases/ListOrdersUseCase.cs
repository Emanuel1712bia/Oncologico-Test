using OrderProcessing.Application.Common;
using OrderProcessing.Application.Orders.Dtos;
using OrderProcessing.Application.Orders.Ports;

namespace OrderProcessing.Application.Orders.UseCases;

public sealed class ListOrdersUseCase
{
    private readonly IOrderRepository _orderRepository;

    public ListOrdersUseCase(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<PagedResult<OrderDetailsResponse>> ExecuteAsync(
        string userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedPageSize = pageSize is < 1 or > 100 ? 20 : pageSize;

        var pagedOrders = await _orderRepository.GetPagedForUserAsync(userId, normalizedPage, normalizedPageSize, cancellationToken);

        var items = pagedOrders.Items.Select(order => order.ToDetailsResponse()).ToList();

        return new PagedResult<OrderDetailsResponse>(items, pagedOrders.Page, pagedOrders.PageSize, pagedOrders.TotalCount);
    }
}
