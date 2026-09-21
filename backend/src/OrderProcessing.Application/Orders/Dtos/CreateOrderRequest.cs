namespace OrderProcessing.Application.Orders.Dtos;

public sealed record CreateOrderRequest(IReadOnlyList<CreateOrderItemRequest> Items);

public sealed record CreateOrderItemRequest(Guid ProductId, int Quantity);
