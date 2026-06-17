using ContactManager.Domain.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ContactManager.API.Middleware;

/// <summary>
/// Translates unhandled exceptions into RFC 7807 problem responses using the
/// .NET 8 <see cref="IExceptionHandler"/> abstraction and the registered
/// <c>ProblemDetails</c> service, replacing the older custom middleware.
/// </summary>
public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        int status;
        string title;
        Dictionary<string, object?>? extensions = null;

        if (exception is ValidationException ve)
        {
            status = StatusCodes.Status400BadRequest;
            var errors = ve.Errors.Select(e => e.ErrorMessage).ToArray();
            title = errors[0];
            if (errors.Length > 1)
                extensions = new Dictionary<string, object?> { ["errors"] = errors };
        }
        else if (exception is NotFoundException)
        {
            status = StatusCodes.Status404NotFound;
            title = exception.Message;
        }
        else
        {
            status = StatusCodes.Status500InternalServerError;
            title = "An unexpected error occurred.";
            logger.LogError(exception, "Unhandled exception");
        }

        httpContext.Response.StatusCode = status;

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Type = $"https://httpstatuses.com/{status}"
        };

        if (extensions is not null)
            foreach (var (key, value) in extensions)
                problem.Extensions[key] = value;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = problem
        });
    }
}
