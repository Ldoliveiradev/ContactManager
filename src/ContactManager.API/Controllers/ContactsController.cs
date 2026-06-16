using ContactManager.API.Extensions;
using ContactManager.Application.Common;
using ContactManager.Application.Contacts.Interfaces;
using ContactManager.Application.Contacts.Models.Requests;
using ContactManager.Application.Contacts.Models.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContactManager.API.Controllers;

[ApiController]
[Route("api/contacts")]
[Authorize]
public sealed class ContactsController(IContactService contacts) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PaginationResponse<ContactListResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PaginationResponse<ContactListResponse>>> GetAll(
        [FromQuery] FilterRequest<GetContactRequest> filter,
        CancellationToken ct = default)
    {
        var result = await contacts.GetAllAsync(User.GetUserId(), filter, ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ContactResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContactResponse>> GetById(Guid id, CancellationToken ct)
    {
        var result = await contacts.GetByIdAsync(User.GetUserId(), new GetContactRequest(id), ct);
        return result.IsSuccess ? Ok(result) : NotFound(result.Error);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ContactResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ContactResponse>> Create(CreateContactRequest request, CancellationToken ct)
    {
        var result = await contacts.CreateAsync(User.GetUserId(), request, ct);
        if (!result.IsSuccess) return BadRequest(result.Error);
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ContactResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContactResponse>> Update(Guid id, UpdateContactRequest request, CancellationToken ct)
    {
        var result = await contacts.UpdateAsync(User.GetUserId(), id, request, ct);
        if (!result.IsSuccess)
            return result.Error == ErrorMessages.Contact.NotFound ? NotFound(result.Error) : BadRequest(result.Error);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await contacts.DeleteAsync(User.GetUserId(), new DeleteContactRequest(id), ct);
        return result.IsSuccess ? NoContent() : NotFound(result.Error);
    }
}
