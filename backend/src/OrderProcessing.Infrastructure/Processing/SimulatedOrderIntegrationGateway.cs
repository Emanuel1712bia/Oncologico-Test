using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderProcessing.Application.Processing.Ports;
using OrderProcessing.Infrastructure.Options;

namespace OrderProcessing.Infrastructure.Processing;

internal sealed class SimulatedOrderIntegrationGateway : IOrderIntegrationGateway
{
    private readonly IntegrationSimulatorOptions _options;
    private readonly ILogger<SimulatedOrderIntegrationGateway> _logger;

    public SimulatedOrderIntegrationGateway(
        IOptions<IntegrationSimulatorOptions> options,
        ILogger<SimulatedOrderIntegrationGateway> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<OrderIntegrationResult> ProcessAsync(OrderIntegrationRequest request, CancellationToken cancellationToken)
    {
        var delaySeconds = Random.Shared.Next(_options.MinDelaySeconds, _options.MaxDelaySeconds + 1);

        _logger.LogInformation(
            "Simulating external integration for order {OrderId}, duration {DelaySeconds}s",
            request.OrderId,
            delaySeconds);

        await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);

        var shouldSucceed = _options.Mode switch
        {
            IntegrationSimulatorMode.AlwaysSucceed => true,
            IntegrationSimulatorMode.AlwaysFail => false,
            _ => Random.Shared.NextDouble() >= _options.FailureRate
        };

        return shouldSucceed
            ? new OrderIntegrationResult(true, null)
            : new OrderIntegrationResult(false, "Simulated external integration failure.");
    }
}
