using E_commerce.Core.Bases;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;

namespace E_commerce.Api.Middleware;

public sealed class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IHostEnvironment environment) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is ValidationException validationException)
        {
            var validationErrors = validationException.Errors
                .Select(error => $"{error.PropertyName}: {error.ErrorMessage}")
                .Distinct()
                .ToArray();
            logger.LogWarning(exception,
                "Request validation failed for {Method} {Path}. TraceId: {TraceId}; Fields: {ValidationFields}",
                httpContext.Request.Method,
                httpContext.Request.Path,
                httpContext.TraceIdentifier,
                validationException.Errors.Select(error => error.PropertyName).Distinct().ToArray());
            httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            httpContext.Response.ContentType = "application/json";
            await httpContext.Response.WriteAsJsonAsync(
                new Response<object>(HttpStatusCode.BadRequest, false, Message: "Request validation failed.", Errors: validationErrors),
                cancellationToken);
            return true;
        }

        logger.LogError(exception,
            "Unhandled exception for {Method} {Path}. TraceId: {TraceId}",
            httpContext.Request.Method,
            httpContext.Request.Path,
            httpContext.TraceIdentifier);

        httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        httpContext.Response.ContentType = "application/json";

        var message = environment.IsDevelopment()
            ? exception.Message
            : "An unexpected error occurred. Please try again later.";

        await httpContext.Response.WriteAsJsonAsync(
            new Response<object>(
                HttpStatusCode.InternalServerError,
                false,
                Message: message,
                Errors: [httpContext.TraceIdentifier]),
            cancellationToken);

        return true;
    }
}
