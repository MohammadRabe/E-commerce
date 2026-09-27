using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace E_commerce.Core.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var operation = typeof(TRequest).Name;
        var timer = Stopwatch.StartNew();

        try
        {
            var response = await next(cancellationToken);
            logger.LogInformation("Handled operation {Operation} in {ElapsedMilliseconds} ms",
                operation, timer.ElapsedMilliseconds);
            return response;
        }
        catch (ValidationException exception)
        {
            logger.LogWarning("Validation rejected operation {Operation}. Failures: {ValidationFailures}",
                operation,
                exception.Errors.Select(error => new { error.PropertyName, error.ErrorMessage }).ToArray());
            throw;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            logger.LogInformation("Operation {Operation} was cancelled after {ElapsedMilliseconds} ms",
                operation, timer.ElapsedMilliseconds);
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Operation {Operation} failed after {ElapsedMilliseconds} ms",
                operation, timer.ElapsedMilliseconds);
            throw;
        }
    }
}
