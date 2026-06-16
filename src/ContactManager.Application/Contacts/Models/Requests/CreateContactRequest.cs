namespace ContactManager.Application.Contacts.Models.Requests;

public record CreateContactRequest(string Name, string Email, string? Phone);
