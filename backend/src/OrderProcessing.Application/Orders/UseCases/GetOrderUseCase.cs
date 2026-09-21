using OrderProcessing.Application.Common;
using OrderProcessing.Application.Orders.Dtos;
using OrderProcessing.Application.Orders.Ports;

namespace OrderProcessing.Application.Orders.UseCases;

public sealed class GetOrderUseCase
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderUseCase(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderDetailsResponse> ExecuteAsync(string userId, Guid orderId, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdForUserAsync(orderId, userId, cancellationToken);
        if (order is null)
        {
            throw new NotFoundAppException($"Order '{orderId}' was not found.");
        }

        return order.ToDetailsResponse();
    }
}
