using Dapper;
using WMS.BuildingBlocks.Infrastructure.Persistence;
using WMS.Modules.Identity.Application.Abstractions;
using WMS.Modules.Identity.Application.Dtos;

namespace WMS.Modules.Identity.Infrastructure.Repositories;

internal sealed class UserReadRepository(ISqlConnectionFactory connectionFactory) : IUserReadRepository
{
    private const string Sql = """
        SELECT u.id AS "Id", u.email AS "Email", u.first_name AS "FirstName", u.last_name AS "LastName",
               r.name AS "RoleName"
        FROM identity.users u
        LEFT JOIN identity.user_roles ur ON ur.user_id = u.id
        LEFT JOIN identity.roles r ON r.id = ur.role_id
        WHERE u.id = @Id
        """;

    public async Task<UserDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        using var connection = connectionFactory.CreateConnection();

        var command = new CommandDefinition(Sql, new { Id = id }, cancellationToken: cancellationToken);
        var rows = (await connection.QueryAsync<UserRoleRow>(command)).ToList();

        if (rows.Count == 0)
        {
            return null;
        }

        var first = rows[0];
        var roles = rows.Where(row => row.RoleName is not null).Select(row => row.RoleName!).ToList();

        return new UserDto(first.Id, first.Email, first.FirstName, first.LastName, roles);
    }

    private sealed record UserRoleRow(Guid Id, string Email, string FirstName, string LastName, string? RoleName);
}
