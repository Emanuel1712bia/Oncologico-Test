namespace OrderProcessing.Domain.Processing;

public sealed class OrderProcessingAttempt
{
    public Guid Id { get; }
    public Guid OrderId { get; }
    public int AttemptNumber { get; }
    public DateTime StartedAtUtc { get; }
    public DateTime FinishedAtUtc { get; }
    public bool Success { get; }
    public string? ErrorMessage { get; }

    private OrderProcessingAttempt(
        Guid id,
        Guid orderId,
        int attemptNumber,
        DateTime startedAtUtc,
        DateTime finishedAtUtc,
        bool success,
        string? errorMessage)
    {
        Id = id;
        OrderId = orderId;
        AttemptNumber = attemptNumber;
        StartedAtUtc = startedAtUtc;
        FinishedAtUtc = finishedAtUtc;
        Success = success;
        ErrorMessage = errorMessage;
    }

    public static OrderProcessingAttempt Create(
        Guid orderId,
        int attemptNumber,
        DateTime startedAtUtc,
        DateTime finishedAtUtc,
        bool success,
        string? errorMessage)
    {
        return new OrderProcessingAttempt(
            Guid.NewGuid(),
            orderId,
            attemptNumber,
            startedAtUtc,
            finishedAtUtc,
            success,
            errorMessage);
    }
}
