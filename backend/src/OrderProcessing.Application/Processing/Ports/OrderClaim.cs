using OrderProcessing.Domain.Orders;

namespace OrderProcessing.Application.Processing.Ports;

public sealed record OrderClaim(Order Order, int AttemptNumber);
