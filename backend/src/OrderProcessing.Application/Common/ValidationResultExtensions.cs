using FluentValidation.Results;

namespace OrderProcessing.Application.Common;

public static class ValidationResultExtensions
{
    public static void ThrowIfInvalid(this ValidationResult validationResult)
    {
        if (validationResult.IsValid)
        {
            return;
        }

        var errors = validationResult.Errors
            .GroupBy(failure => failure.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(failure => failure.ErrorMessage).ToArray());

        throw new ValidationAppException(errors);
    }
}
