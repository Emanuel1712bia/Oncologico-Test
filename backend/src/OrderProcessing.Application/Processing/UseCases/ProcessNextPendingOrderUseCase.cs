using Microsoft.Extensions.Logging;
using OrderProcessing.Application.Processing.Ports;
using OrderProcessing.Domain.Processing;

namespace OrderProcessing.Application.Processing.UseCases;

public sealed class ProcessNextPendingOrderUseCase
{
    private readonly IOrderProcessingRepository _orderProcessingRepository;
    private readonly IOrderIntegrationGateway _orderIntegrationGateway;
    private readonly ILogger<ProcessNextPendingOrderUseCase> _logger;

    public ProcessNextPendingOrderUseCase(
        IOrderProcessingRepository orderProcessingRepository,
        IOrderIntegrationGateway orderIntegrationGateway,
        ILogger<ProcessNextPendingOrderUseCase> logger)
    {
        _orderProcessingRepository = orderProcessingRepository;
        _orderIntegrationGateway = orderIntegrationGateway;
        _logger = logger;
    }

    public async Task<bool> ExecuteAsync(CancellationToken cancellationToken)
    {
        var claim = await _orderProcessingRepository.ClaimNextPendingOrderAsync(cancellationToken);
        if (claim is null)
        {
            return false;
        }

        _logger.LogInformation(
            "Starting processing attempt {AttemptNumber} for order {OrderId}",
            claim.AttemptNumber,
            claim.Order.Id);

        var startedAtUtc = DateTime.UtcNow;

        OrderIntegrationResult result;
        try
        {
            result = await _orderIntegrationGateway.ProcessAsync(
                new OrderIntegrationRequest(claim.Order.Id, claim.Order.TotalAmount),
                cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            _logger.LogError(
                exception,
                "Unhandled error while processing order {OrderId} attempt {AttemptNumber}",
                claim.Order.Id,
                claim.AttemptNumber);
            result = new OrderIntegrationResult(false, exception.Message);
        }

        var finishedAtUtc = DateTime.UtcNow;

        var attempt = OrderProcessingAttempt.Create(
            claim.Order.Id,
            claim.AttemptNumber,
            startedAtUtc,
            finishedAtUtc,
            result.Success,
            result.ErrorMessage);

        await _orderProcessingRepository.FinalizeAttemptAsync(attempt, result.Success, cancellationToken);

        _logger.LogInformation(
            "Finished processing order {OrderId}: {Outcome}",
            claim.Order.Id,
            result.Success ? "Completed" : "Failed");

        return true;
    }
}
