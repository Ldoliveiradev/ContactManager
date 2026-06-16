using System.Text;
using System.Text.Json;
using ContactManager.API.Middleware;
using ContactManager.Domain.Exceptions;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace ContactManager.API.Tests;

public class ExceptionHandlingMiddlewareTests
{
    private static async Task<(int status, string contentType, string body)> InvokeWith(Exception toThrow)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var middleware = new ExceptionHandlingMiddleware(
            _ => throw toThrow,
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body, Encoding.UTF8).ReadToEndAsync();
        return (context.Response.StatusCode, context.Response.ContentType ?? "", body);
    }

    [Fact]
    public async Task PassesThrough_WhenNoException()
    {
        var context = new DefaultHttpContext();
        var called = false;
        var middleware = new ExceptionHandlingMiddleware(
            _ => { called = true; return Task.CompletedTask; },
            NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        called.Should().BeTrue();
        context.Response.StatusCode.Should().Be(StatusCodes.Status200OK);
    }

    [Fact]
    public async Task ValidationException_Maps_To400_WithFirstError()
    {
        var ex = new ValidationException(new[]
        {
            new ValidationFailure("Email", "Email is invalid."),
        });

        var (status, contentType, body) = await InvokeWith(ex);

        status.Should().Be(StatusCodes.Status400BadRequest);
        contentType.Should().Contain("application/problem+json");
        TitleOf(body).Should().Be("Email is invalid.");
    }

    [Fact]
    public async Task NotFoundException_Maps_To404()
    {
        var (status, contentType, body) = await InvokeWith(new ContactNotFoundException());

        status.Should().Be(StatusCodes.Status404NotFound);
        contentType.Should().Contain("application/problem+json");
        TitleOf(body).Should().Be("Contact not found.");
    }

    [Fact]
    public async Task UnhandledException_Maps_To500_WithGenericTitle()
    {
        var (status, contentType, body) = await InvokeWith(new InvalidOperationException("boom"));

        status.Should().Be(StatusCodes.Status500InternalServerError);
        contentType.Should().Contain("application/problem+json");
        // The raw exception message must not leak; a generic title is returned.
        TitleOf(body).Should().Be("An unexpected error occurred.");
        body.Should().NotContain("boom");
    }

    private static string TitleOf(string problemJson) =>
        JsonDocument.Parse(problemJson).RootElement.GetProperty("title").GetString()!;
}
