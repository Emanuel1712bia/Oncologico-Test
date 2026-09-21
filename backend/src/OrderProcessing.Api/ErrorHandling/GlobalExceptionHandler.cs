using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using OrderProcessing.Application.Common;
using OrderProcessing.Domain.Common;

namespace OrderProcessing.Api.ErrorHandling;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        switch (exception)
        {
            case ValidationAppException validationAppException:
                _logger.LogWarning(exception, "Validation error handling {Path}", httpContext.Request.Path);
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await httpContext.Response.WriteAsJsonAsync(
                    new ValidationProblemDetails(new Dictionary<string, string[]>(validationAppException.Errors))
                    {
                        Title = "One or more validation errors occurred.",
                        Status = StatusCodes.Status400BadRequest
                    },
                    cancellationToken);
                return true;

            case NotFoundAppException notFoundAppException:
                _logger.LogWarning(exception, "Not found handling {Path}", httpContext.Request.Path);
                httpContext.Response.StatusCode = StatusCodes.Status404NotFound;
                await httpContext.Response.WriteAsJsonAsync(
                    new ProblemDetails
                    {
                        Title = "Resource not found.",
                        Detail = notFoundAppException.Message,
                        Status = StatusCodes.Status404NotFound
                    },
                    cancellationToken);
                return true;

            case DomainException domainException:
                _logger.LogWarning(exception, "Domain rule violation handling {Path}", httpContext.Request.Path);
                httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                await httpContext.Response.WriteAsJsonAsync(
                    new ProblemDetails
                    {
                        Title = "Business rule violation.",
                        Detail = domainException.Message,
                        Status = StatusCodes.Status400BadRequest
                    },
                    cancellationToken);
                return true;

            default:
                _logger.LogError(exception, "Unhandled exception handling {Path}", httpContext.Request.Path);
                return false;
        }
    }
}
