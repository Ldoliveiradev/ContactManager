namespace ContactManager.Application.Contacts.Models.Dto;

public sealed record ContactDto(Guid Id, string Name, string Email, string? Phone);
