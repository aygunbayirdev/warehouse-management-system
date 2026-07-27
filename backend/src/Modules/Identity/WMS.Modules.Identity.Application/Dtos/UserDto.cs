namespace WMS.Modules.Identity.Application.Dtos;

public sealed record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    IReadOnlyCollection<string> Roles);
