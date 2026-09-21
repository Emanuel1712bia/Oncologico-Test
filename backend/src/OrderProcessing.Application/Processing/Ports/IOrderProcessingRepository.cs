using OrderProcessing.Domain.Processing;

namespace OrderProcessing.Application.Processing.Ports;

public interface IOrderProcessingRepository
{
    Task<OrderClaim?> ClaimNextPendingOrderAsync(CancellationToken cancellationToken);

    Task FinalizeAttemptAsync(OrderProcessingAttempt attempt, bool orderSucceeded, CancellationToken cancellationToken);
}
