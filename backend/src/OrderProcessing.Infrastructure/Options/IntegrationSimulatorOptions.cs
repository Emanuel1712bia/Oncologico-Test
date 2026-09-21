namespace OrderProcessing.Infrastructure.Options;

public enum IntegrationSimulatorMode
{
    Random,
    AlwaysSucceed,
    AlwaysFail
}

public sealed class IntegrationSimulatorOptions
{
    public const string SectionName = "IntegrationSimulator";

    public int MinDelaySeconds { get; set; } = 5;

    public int MaxDelaySeconds { get; set; } = 10;

    public IntegrationSimulatorMode Mode { get; set; } = IntegrationSimulatorMode.Random;

    public double FailureRate { get; set; } = 0.3;
}
