using ContactManager.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace ContactManager.API.Middleware;

/// <summary>
/// Translates Application-layer exceptions into RFC 7807 ProblemDetails with the right
/// HTTP status code, so controllers stay thin and never leak stack traces. Keeps the
/// status-code policy in one place: validation → 400, bad credentials → 401,
/// duplicate username → 409, anything else → 500.
/// </summary>
public sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            var (status, title) = ex switch
            {
                ValidationException => (StatusCodes.Status400BadRequest, ex.Message),
                InvalidCredentialsException => (StatusCodes.Status401Unauthorized, ex.Message),
                UsernameAlreadyExistsException => (StatusCodes.Status409Conflict, ex.Message),
                _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
            };

            if (status == StatusCodes.Status500InternalServerError)
                logger.LogError(ex, "Unhandled exception");

            var problem = new ProblemDetails
            {
                Status = status,
                Title = title,
                Type = $"https://httpstatuses.com/{status}"
            };

            context.Response.StatusCode = status;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(problem);
        }
    }
}
