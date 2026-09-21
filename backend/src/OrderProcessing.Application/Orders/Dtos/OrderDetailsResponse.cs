namespace OrderProcessing.Application.Orders.Dtos;

public sealed record OrderDetailsResponse(
    Guid Id,
    string UserId,
    string Status,
    decimal TotalAmount,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    IReadOnlyList<OrderItemResponse> Items);

public sealed record OrderItemResponse(
    Guid ProductId,
    string ProductName,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal);
