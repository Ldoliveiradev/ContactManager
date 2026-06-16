using ContactManager.API.Extensions;
using ContactManager.Application.Contacts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContactManager.API.Controllers;

/// <summary>
/// CRUD endpoints for the authenticated user's contacts. The whole controller requires
/// authentication ([Authorize]) — these are the "authorized" endpoints the brief asks for.
/// The owning user id is always read from the JWT, never from the request.
/// </summary>
[ApiController]
[Route("api/contacts")]
[Authorize]
public sealed class ContactsController(IContactService contacts) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ContactResponse>>> GetAll(CancellationToken ct)
        => Ok(await contacts.GetAllAsync(User.GetUserId(), ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ContactResponse>> GetById(Guid id, CancellationToken ct)
        => Ok(await contacts.GetByIdAsync(User.GetUserId(), id, ct));

    [HttpPost]
    public async Task<ActionResult<ContactResponse>> Create(CreateContactRequest request, CancellationToken ct)
    {
        var created = await contacts.CreateAsync(User.GetUserId(), request, ct);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ContactResponse>> Update(Guid id, UpdateContactRequest request, CancellationToken ct)
        => Ok(await contacts.UpdateAsync(User.GetUserId(), id, request, ct));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await contacts.DeleteAsync(User.GetUserId(), id, ct);
        return NoContent();
    }
}
