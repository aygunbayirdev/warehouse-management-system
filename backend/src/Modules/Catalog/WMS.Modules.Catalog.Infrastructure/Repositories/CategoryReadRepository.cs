using Dapper;
using WMS.BuildingBlocks.Infrastructure.Persistence;
using WMS.Modules.Catalog.Application.Abstractions;
using WMS.Modules.Catalog.Application.Dtos;

namespace WMS.Modules.Catalog.Infrastructure.Repositories;

internal sealed class CategoryReadRepository(ISqlConnectionFactory connectionFactory) : ICategoryReadRepository
{
    public async Task<CategoryDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT id AS "Id", name AS "Name"
            FROM catalog.categories
            WHERE id = @Id
            """;

        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken);

        return await connection.QueryFirstOrDefaultAsync<CategoryDto>(command);
    }

    public async Task<IReadOnlyCollection<CategoryDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT id AS "Id", name AS "Name"
            FROM catalog.categories
            ORDER BY name
            """;

        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, cancellationToken: cancellationToken);

        var result = await connection.QueryAsync<CategoryDto>(command);

        return result.ToList();
    }
}
