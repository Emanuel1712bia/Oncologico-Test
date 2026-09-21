namespace OrderProcessing.Application.Orders.Dtos;

public sealed record OrderAcceptedResponse(Guid OrderId, string Status);
