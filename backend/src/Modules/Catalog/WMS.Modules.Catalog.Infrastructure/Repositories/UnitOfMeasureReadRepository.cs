using Dapper;
using WMS.BuildingBlocks.Infrastructure.Persistence;
using WMS.Modules.Catalog.Application.Abstractions;
using WMS.Modules.Catalog.Application.Dtos;

namespace WMS.Modules.Catalog.Infrastructure.Repositories;

internal sealed class UnitOfMeasureReadRepository(ISqlConnectionFactory connectionFactory) : IUnitOfMeasureReadRepository
{
    public async Task<UnitOfMeasureDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT id AS "Id", code AS "Code", name AS "Name"
            FROM catalog.units_of_measure
            WHERE id = @Id
            """;

        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken);

        return await connection.QueryFirstOrDefaultAsync<UnitOfMeasureDto>(command);
    }

    public async Task<IReadOnlyCollection<UnitOfMeasureDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT id AS "Id", code AS "Code", name AS "Name"
            FROM catalog.units_of_measure
            ORDER BY name
            """;

        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, cancellationToken: cancellationToken);

        var result = await connection.QueryAsync<UnitOfMeasureDto>(command);

        return result.ToList();
    }
}
