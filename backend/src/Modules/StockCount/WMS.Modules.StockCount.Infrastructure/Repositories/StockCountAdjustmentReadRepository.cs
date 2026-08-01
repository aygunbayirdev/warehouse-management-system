using System.Text;
using Dapper;
using WMS.BuildingBlocks.Application.Models;
using WMS.BuildingBlocks.Infrastructure.Persistence;
using WMS.Modules.StockCount.Application.Abstractions;
using WMS.Modules.StockCount.Application.Dtos;
using WMS.Modules.StockCount.Domain;

namespace WMS.Modules.StockCount.Infrastructure.Repositories;

/// <summary>
/// Reporting query joining stockcount.stock_count_adjustments with catalog.products and
/// inventory.warehouses across schemas — the pragmatic Dapper-only exception documented in
/// CLAUDE.md's CQRS section.
/// </summary>
internal sealed class StockCountAdjustmentReadRepository(ISqlConnectionFactory connectionFactory) : IStockCountAdjustmentReadRepository
{
    private const string BaseSql = """
        SELECT sca.id AS "Id", sca.stock_count_id AS "StockCountId",
               sca.warehouse_id AS "WarehouseId", w.name AS "WarehouseName",
               sca.product_id AS "ProductId", p.sku AS "ProductSku", p.name AS "ProductName",
               sca.difference_quantity AS "DifferenceQuantity", sca.status AS "Status",
               sca.approved_by_user_id AS "ApprovedByUserId",
               sca.created_at_utc AS "CreatedAtUtc", sca.decided_at_utc AS "DecidedAtUtc"
        FROM stockcount.stock_count_adjustments sca
        INNER JOIN inventory.warehouses w ON w.id = sca.warehouse_id
        INNER JOIN catalog.products p ON p.id = sca.product_id
        """;

    public async Task<StockCountAdjustmentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var sql = $"{BaseSql} WHERE sca.id = @Id";

        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<StockCountAdjustmentDto>(command);
    }

    public async Task<PagedResult<StockCountAdjustmentDto>> GetListAsync(
        Guid? warehouseId,
        StockCountAdjustmentStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var whereClause = new StringBuilder();
        var parameters = new DynamicParameters();
        var hasWhere = false;

        if (warehouseId is not null)
        {
            whereClause.Append(" WHERE sca.warehouse_id = @WarehouseId");
            parameters.Add("WarehouseId", warehouseId);
            hasWhere = true;
        }

        if (status is not null)
        {
            whereClause.Append(hasWhere ? " AND " : " WHERE ");
            whereClause.Append("sca.status = @Status");
            parameters.Add("Status", status.Value.ToString());
        }

        parameters.Add("PageSize", pageSize);
        parameters.Add("Skip", (page - 1) * pageSize);

        var sql = $"""
            SELECT COUNT(*) FROM stockcount.stock_count_adjustments sca{whereClause};
            {BaseSql}{whereClause} ORDER BY sca.created_at_utc DESC LIMIT @PageSize OFFSET @Skip;
            """;

        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

        await using var multi = await connection.QueryMultipleAsync(command);
        var totalCount = await multi.ReadSingleAsync<int>();
        var items = (await multi.ReadAsync<StockCountAdjustmentDto>()).ToList();

        return new PagedResult<StockCountAdjustmentDto>(items, totalCount, page, pageSize);
    }
}
