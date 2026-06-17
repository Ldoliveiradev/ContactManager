using System.Text;
using System.Text.Json;
using ContactManager.API.Middleware;
using ContactManager.Domain.Exceptions;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace ContactManager.API.Tests;

public class GlobalExceptionHandlerTests
{
    private static async Task<(int status, string contentType, string body)> Handle(Exception toThrow)
    {
        // The handler depends on the framework's IProblemDetailsService, which
        // AddProblemDetails() registers — build a minimal provider for it.
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddProblemDetails();
        var provider = services.BuildServiceProvider();
        var problemDetailsService = provider.GetRequiredService<IProblemDetailsService>();

        var context = new DefaultHttpContext { RequestServices = provider };
        context.Response.Body = new MemoryStream();
        // ProblemDetailsService negotiates the response format off the request's
        // accepted media types; mark the endpoint as accepting JSON.
        context.Request.Headers.Accept = "application/json";

        var handler = new GlobalExceptionHandler(
            problemDetailsService, NullLogger<GlobalExceptionHandler>.Instance);

        var handled = await handler.TryHandleAsync(context, toThrow, CancellationToken.None);
        handled.Should().BeTrue();

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await new StreamReader(context.Response.Body, Encoding.UTF8).ReadToEndAsync();
        return (context.Response.StatusCode, context.Response.ContentType ?? "", body);
    }

    [Fact]
    public async Task ValidationException_Maps_To400_WithFirstError()
    {
        // Arrange
        var ex = new ValidationException(new[]
        {
            new ValidationFailure("Email", "Email is invalid."),
        });

        // Act
        var (status, contentType, body) = await Handle(ex);

        // Assert
        status.Should().Be(StatusCodes.Status400BadRequest);
        contentType.Should().Contain("application/problem+json");
        TitleOf(body).Should().Be("Email is invalid.");
    }

    [Fact]
    public async Task NotFoundException_Maps_To404()
    {
        // Arrange
        var ex = new ContactNotFoundException();

        // Act
        var (status, contentType, body) = await Handle(ex);

        // Assert
        status.Should().Be(StatusCodes.Status404NotFound);
        contentType.Should().Contain("application/problem+json");
        TitleOf(body).Should().Be("Contact not found.");
    }

    [Fact]
    public async Task UnhandledException_Maps_To500_WithGenericTitle()
    {
        // Arrange
        var ex = new InvalidOperationException("boom");

        // Act
        var (status, contentType, body) = await Handle(ex);

        // Assert
        status.Should().Be(StatusCodes.Status500InternalServerError);
        contentType.Should().Contain("application/problem+json");
        // The raw exception message must not leak; a generic title is returned.
        TitleOf(body).Should().Be("An unexpected error occurred.");
        body.Should().NotContain("boom");
    }

    private static string TitleOf(string problemJson) =>
        JsonDocument.Parse(problemJson).RootElement.GetProperty("title").GetString()!;
}
