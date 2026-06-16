using ContactManager.API.Extensions;
using ContactManager.Application.Accounts.Interfaces;
using ContactManager.Application.Accounts.Models.Requests;
using ContactManager.Application.Accounts.Models.Responses;
using ContactManager.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContactManager.API.Controllers;

[ApiController]
[Route("api/accounts")]
[Authorize]
public sealed class AccountsController(IAccountService accounts) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(AccountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountResponse>> Get(CancellationToken ct)
    {
        var accountId = User.GetUserId();
        var result = await accounts.GetByIdAsync(accountId, new GetAccountRequest(accountId), ct);
        return result.IsSuccess ? Ok(result) : NotFound(result.Error);
    }

    [HttpPut]
    [ProducesResponseType(typeof(AccountResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccountResponse>> UpdateProfile(UpdateAccountRequest request, CancellationToken ct)
    {
        var result = await accounts.UpdateProfileAsync(User.GetUserId(), request, ct);
        if (!result.IsSuccess)
            return result.Error == ErrorMessages.Account.NotFound
                ? NotFound(result.Error)
                : BadRequest(result.Error);
        return Ok(result);
    }

    [HttpPut("password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePassword(UpdatePasswordRequest request, CancellationToken ct)
    {
        var result = await accounts.UpdatePasswordAsync(User.GetUserId(), request, ct);
        if (!result.IsSuccess)
            return result.Error == ErrorMessages.Account.NotFound
                ? NotFound(result.Error)
                : BadRequest(result.Error);
        return NoContent();
    }
}
