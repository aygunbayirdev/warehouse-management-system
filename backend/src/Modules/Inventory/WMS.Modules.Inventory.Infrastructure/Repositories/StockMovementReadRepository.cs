using System.Text;
using Dapper;
using WMS.BuildingBlocks.Application.Models;
using WMS.BuildingBlocks.Infrastructure.Persistence;
using WMS.Modules.Inventory.Application.Abstractions;
using WMS.Modules.Inventory.Application.Dtos;

namespace WMS.Modules.Inventory.Infrastructure.Repositories;

/// <summary>
/// Reporting query joining inventory.stock_movements with inventory.warehouses/catalog.products
/// across schemas — the pragmatic Dapper-only exception documented in CLAUDE.md's CQRS section.
/// This is the audit ledger report: every stock quantity change, across all modules, ends up here.
/// </summary>
internal sealed class StockMovementReadRepository(ISqlConnectionFactory connectionFactory) : IStockMovementReadRepository
{
    private const string BaseSql = """
        SELECT sm.id AS "Id", sm.warehouse_id AS "WarehouseId", w.code AS "WarehouseCode", w.name AS "WarehouseName",
               sm.product_id AS "ProductId", p.sku AS "ProductSku", p.name AS "ProductName",
               sm.type AS "Type", sm.quantity AS "Quantity", sm.reason AS "Reason", sm.occurred_at_utc AS "OccurredAtUtc"
        FROM inventory.stock_movements sm
        INNER JOIN inventory.warehouses w ON w.id = sm.warehouse_id
        INNER JOIN catalog.products p ON p.id = sm.product_id
        """;

    public async Task<PagedResult<StockMovementDto>> GetListAsync(
        Guid? warehouseId,
        Guid? productId,
        DateTime? fromUtc,
        DateTime? toUtc,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var whereClause = new StringBuilder();
        var parameters = new DynamicParameters();
        var hasWhere = false;

        if (warehouseId is not null)
        {
            whereClause.Append(" WHERE sm.warehouse_id = @WarehouseId");
            parameters.Add("WarehouseId", warehouseId);
            hasWhere = true;
        }

        if (productId is not null)
        {
            whereClause.Append(hasWhere ? " AND " : " WHERE ");
            whereClause.Append("sm.product_id = @ProductId");
            parameters.Add("ProductId", productId);
            hasWhere = true;
        }

        if (fromUtc is not null)
        {
            whereClause.Append(hasWhere ? " AND " : " WHERE ");
            whereClause.Append("sm.occurred_at_utc >= @FromUtc");
            parameters.Add("FromUtc", fromUtc);
            hasWhere = true;
        }

        if (toUtc is not null)
        {
            whereClause.Append(hasWhere ? " AND " : " WHERE ");
            whereClause.Append("sm.occurred_at_utc <= @ToUtc");
            parameters.Add("ToUtc", toUtc);
        }

        parameters.Add("PageSize", pageSize);
        parameters.Add("Skip", (page - 1) * pageSize);

        var sql = $"""
            SELECT COUNT(*) FROM inventory.stock_movements sm{whereClause};
            {BaseSql}{whereClause} ORDER BY sm.occurred_at_utc DESC LIMIT @PageSize OFFSET @Skip;
            """;

        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

        await using var multi = await connection.QueryMultipleAsync(command);
        var totalCount = await multi.ReadSingleAsync<int>();
        var items = (await multi.ReadAsync<StockMovementDto>()).ToList();

        return new PagedResult<StockMovementDto>(items, totalCount, page, pageSize);
    }
}
