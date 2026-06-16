using ContactManager.Domain.Exceptions;
using ContactManager.Infrastructure.Identity.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace ContactManager.API.Middleware;

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
                ValidationException ve => (StatusCodes.Status400BadRequest, ve.Errors.First().ErrorMessage),
                InvalidCredentialsException => (StatusCodes.Status401Unauthorized, ex.Message),
                NotFoundException => (StatusCodes.Status404NotFound, ex.Message),
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
