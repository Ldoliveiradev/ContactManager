using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ContactManager.API.Extensions;

/// <summary>
/// Helpers that turn an Application-layer error string into an RFC 7807
/// ProblemDetails response, so expected failures (validation, not-found,
/// conflicts) return the same problem+json shape as unhandled exceptions —
/// without throwing for control flow.
/// </summary>
public static class ProblemResultExtensions
{
    public static ObjectResult Problem(this ControllerBase controller, int statusCode, string? error) =>
        controller.Problem(detail: error, statusCode: statusCode);

    public static ObjectResult ValidationProblem(this ControllerBase controller, string? error) =>
        controller.Problem(StatusCodes.Status400BadRequest, error);

    public static ObjectResult NotFoundProblem(this ControllerBase controller, string? error) =>
        controller.Problem(StatusCodes.Status404NotFound, error);

    public static ObjectResult ConflictProblem(this ControllerBase controller, string? error) =>
        controller.Problem(StatusCodes.Status409Conflict, error);

    public static ObjectResult UnauthorizedProblem(this ControllerBase controller, string? error) =>
        controller.Problem(StatusCodes.Status401Unauthorized, error);

    public static ObjectResult ForbiddenProblem(this ControllerBase controller, string? error) =>
        controller.Problem(StatusCodes.Status403Forbidden, error);
}
