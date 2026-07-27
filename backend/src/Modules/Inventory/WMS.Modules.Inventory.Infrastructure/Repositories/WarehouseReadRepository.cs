using Dapper;
using WMS.BuildingBlocks.Infrastructure.Persistence;
using WMS.Modules.Inventory.Application.Abstractions;
using WMS.Modules.Inventory.Application.Dtos;

namespace WMS.Modules.Inventory.Infrastructure.Repositories;

internal sealed class WarehouseReadRepository(ISqlConnectionFactory connectionFactory) : IWarehouseReadRepository
{
    public async Task<WarehouseDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT id AS "Id", code AS "Code", name AS "Name", address AS "Address"
            FROM inventory.warehouses
            WHERE id = @Id
            """;

        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken);

        return await connection.QueryFirstOrDefaultAsync<WarehouseDto>(command);
    }

    public async Task<IReadOnlyCollection<WarehouseDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT id AS "Id", code AS "Code", name AS "Name", address AS "Address"
            FROM inventory.warehouses
            ORDER BY name
            """;

        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, cancellationToken: cancellationToken);

        var result = await connection.QueryAsync<WarehouseDto>(command);

        return result.ToList();
    }
}
