namespace OrderProcessing.Worker.Options;

public sealed class WorkerOptions
{
    public const string SectionName = "Worker";

    public int PollingIntervalSeconds { get; set; } = 2;

    public int ErrorBackoffSeconds { get; set; } = 5;
}
