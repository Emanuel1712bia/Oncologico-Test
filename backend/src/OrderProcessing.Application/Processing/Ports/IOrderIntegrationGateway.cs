namespace OrderProcessing.Application.Processing.Ports;

public sealed record OrderIntegrationRequest(Guid OrderId, decimal TotalAmount);

public sealed record OrderIntegrationResult(bool Success, string? ErrorMessage);

public interface IOrderIntegrationGateway
{
    Task<OrderIntegrationResult> ProcessAsync(OrderIntegrationRequest request, CancellationToken cancellationToken);
}
