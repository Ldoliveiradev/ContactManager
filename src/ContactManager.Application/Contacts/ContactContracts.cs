namespace ContactManager.Application.Contacts;

public record CreateContactRequest(string Name, string Email, string? Phone);

public record UpdateContactRequest(string Name, string Email, string? Phone);

/// <summary>Contact data returned to API callers (no owner id is exposed).</summary>
public record ContactResponse(Guid Id, string Name, string Email, string? Phone);
