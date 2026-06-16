namespace ContactManager.Application.Contacts.Models.Requests;

public record UpdateContactRequest(string Name, string Email, string? Phone);
