using Microsoft.Extensions.Options;
using OrderProcessing.Application.Processing.UseCases;
using OrderProcessing.Worker.Options;

namespace OrderProcessing.Worker;

public sealed class OrderProcessingWorker : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly WorkerOptions _options;
    private readonly ILogger<OrderProcessingWorker> _logger;

    public OrderProcessingWorker(
        IServiceScopeFactory serviceScopeFactory,
        IOptions<WorkerOptions> options,
        ILogger<OrderProcessingWorker> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Order processing worker started");

        while (!stoppingToken.IsCancellationRequested)
        {
            bool processedAnOrder;

            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var useCase = scope.ServiceProvider.GetRequiredService<ProcessNextPendingOrderUseCase>();
                processedAnOrder = await useCase.ExecuteAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Unexpected error while polling for pending orders");
                await DelayAsync(_options.ErrorBackoffSeconds, stoppingToken);
                continue;
            }

            if (!processedAnOrder)
            {
                await DelayAsync(_options.PollingIntervalSeconds, stoppingToken);
            }
        }

        _logger.LogInformation("Order processing worker stopped");
    }

    private static async Task DelayAsync(int seconds, CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(seconds), cancellationToken);
        }
        catch (OperationCanceledException)
        {
        }
    }
}
