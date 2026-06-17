namespace ContactManager.Application.Accounts.Models.Dto;

public sealed record AccountDto(Guid Id, string FirstName, string LastName, string Email);
