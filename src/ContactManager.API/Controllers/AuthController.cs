using ContactManager.API.Extensions;
using ContactManager.Application.Auth.Interfaces;
using ContactManager.Application.Auth.Models;
using ContactManager.Application.Auth.Models.Requests;
using ContactManager.Application.Auth.Models.Responses;
using ContactManager.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContactManager.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(IAuthService auth) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RegisterResponse>> Register(RegisterRequest request, CancellationToken ct)
    {
        var result = await auth.RegisterAsync(request, ct);
        if (!result.IsSuccess)
        {
            var isConflict = result.Error!.Contains("already taken")
                || result.Error.Contains("already exists");
            return isConflict ? this.ConflictProblem(result.Error) : this.ValidationProblem(result.Error);
        }
        return CreatedAtAction(nameof(Register), new { id = result.Data!.Id }, result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken ct)
    {
        var result = await auth.LoginAsync(request, ct);
        return result.IsSuccess ? Ok(result) : this.UnauthorizedProblem(result.Error);
    }

    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePassword(ChangePasswordRequest request, CancellationToken ct)
    {
        var authenticatedUserId = User.GetUserId();
        if (request.UserId != authenticatedUserId)
            return this.ForbiddenProblem(ErrorMessages.Auth.Forbidden);

        var result = await auth.ChangePasswordAsync(authenticatedUserId, request, ct);
        if (!result.IsSuccess)
            return result.Error == ErrorMessages.Auth.UserNotFound
                ? this.NotFoundProblem(result.Error)
                : this.ValidationProblem(result.Error);
        return NoContent();
    }
}
