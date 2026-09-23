using E_commerce.Core.Bases;
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
