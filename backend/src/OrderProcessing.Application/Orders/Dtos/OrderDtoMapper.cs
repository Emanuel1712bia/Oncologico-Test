using OrderProcessing.Domain.Orders;

namespace OrderProcessing.Application.Orders.Dtos;

public static class OrderDtoMapper
{
    public static OrderDetailsResponse ToDetailsResponse(this Order order)
    {
        var items = order.Items
            .Select(item => new OrderItemResponse(item.ProductId, item.ProductName, item.UnitPrice, item.Quantity, item.LineTotal))
            .ToList();

        return new OrderDetailsResponse(
            order.Id,
            order.UserId,
            order.Status.ToString(),
            order.TotalAmount,
            order.CreatedAtUtc,
            order.UpdatedAtUtc,
            items);
    }
}
