namespace OrderProcessing.Application.Common;

public sealed class ValidationAppException : Exception
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationAppException(IReadOnlyDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.")
    {
        Errors = errors;
    }
}

public sealed class NotFoundAppException : Exception
{
    public NotFoundAppException(string message) : base(message)
    {
    }
}
