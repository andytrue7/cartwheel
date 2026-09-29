using Cartwheel.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Cartwheel.Api.ErrorHandling;

/// <summary>
/// Turns domain exceptions into 4xx ProblemDetails responses. Anything else is left to the
/// default exception handler, which answers with a generic 500.
/// </summary>
public sealed class DomainExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<DomainExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not DomainException domainException)
        {
            return false;
        }

        var (status, title) = domainException switch
        {
            InsufficientStockException => (StatusCodes.Status409Conflict, "Insufficient stock"),
            StockLimitExceededException => (StatusCodes.Status409Conflict, "Stock limit exceeded"),
            ProductNotFoundException => (StatusCodes.Status404NotFound, "Product not found"),
            _ => (StatusCodes.Status400BadRequest, "Business rule violated")
        };

        // A business outcome, not a crash: Warning keeps real errors easy to find in the logs.
        logger.LogWarning(domainException, "Domain rule violated: {Title}", title);

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = domainException.Message
        };

        if (domainException is InsufficientStockException stock)
        {
            problem.Extensions["requestedQuantity"] = stock.RequestedQuantity;
            problem.Extensions["availableQuantity"] = stock.AvailableQuantity;
        }
        else if (domainException is StockLimitExceededException limit)
        {
            problem.Extensions["requestedQuantity"] = limit.RequestedQuantity;
            problem.Extensions["currentQuantity"] = limit.CurrentQuantity;
            problem.Extensions["maxQuantity"] = limit.MaxQuantity;
        }

        httpContext.Response.StatusCode = status;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = domainException
        });
    }
}
