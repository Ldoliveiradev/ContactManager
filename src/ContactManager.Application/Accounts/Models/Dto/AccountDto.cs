namespace ContactManager.Application.Accounts.Models.Dto;

public sealed record AccountDto(Guid Id, string Username, string FirstName, string LastName, string Email);
