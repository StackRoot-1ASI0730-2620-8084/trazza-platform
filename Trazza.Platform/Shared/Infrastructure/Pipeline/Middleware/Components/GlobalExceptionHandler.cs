using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Trazza.Platform.Shared.Resources.Errors;

namespace Trazza.Platform.Shared.Infrastructure.Pipeline.Middleware.Components;

/// <summary>
///     Turns every unhandled exception into an RFC 9457 ProblemDetails response with a stable error code.
///     Expected business errors never reach this handler: they travel as Result failures.
/// </summary>
public class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    IStringLocalizer<ErrorMessages> localizer,
    IHostEnvironment environment,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, code, defaultTitle) = Classify(exception);

        if (status >= StatusCodes.Status500InternalServerError)
            logger.LogError(exception, "Unhandled exception on {Method} {Path}", httpContext.Request.Method,
                httpContext.Request.Path);
        else
            logger.LogWarning(exception, "Request {Method} {Path} failed with {ErrorCode}", httpContext.Request.Method,
                httpContext.Request.Path, code);

        var title = localizer[code];
        var problemDetails = new ProblemDetails
        {
            Status = status,
            Title = title.ResourceNotFound ? defaultTitle : title.Value,
            Detail = environment.IsDevelopment() ? exception.Message : null,
            Instance = httpContext.Request.Path
        };
        problemDetails.Extensions["code"] = code;

        httpContext.Response.StatusCode = status;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception
        });
    }

    private static (int Status, string Code, string DefaultTitle) Classify(Exception exception)
    {
        return exception switch
        {
            DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, "Shared.ConcurrencyConflict",
                "The data was changed by another request. Reload and try again."),
            DbUpdateException => (StatusCodes.Status409Conflict, "Shared.DatabaseConflict",
                "The change conflicts with the data already stored."),
            BadHttpRequestException => (StatusCodes.Status400BadRequest, "Shared.BadRequest",
                "The request is not valid."),
            OperationCanceledException => (StatusCodes.Status400BadRequest, "Shared.RequestCancelled",
                "The request was cancelled."),
            _ => (StatusCodes.Status500InternalServerError, "Shared.Unexpected",
                "An unexpected error occurred.")
        };
    }
}
