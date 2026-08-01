using System.Text;
using Dapper;
using WMS.BuildingBlocks.Infrastructure.Persistence;
using WMS.Modules.Inventory.Application.Abstractions;
using WMS.Modules.Inventory.Application.Dtos;

namespace WMS.Modules.Inventory.Infrastructure.Repositories;

/// <summary>
/// Reporting query joining inventory.stock_items with catalog.products/units_of_measure across
/// schemas — the pragmatic Dapper-only exception documented in CLAUDE.md's CQRS section.
/// </summary>
internal sealed class StockItemReadRepository(ISqlConnectionFactory connectionFactory) : IStockItemReadRepository
{
    private const string BaseSql = """
        SELECT si.warehouse_id AS "WarehouseId", w.code AS "WarehouseCode", w.name AS "WarehouseName",
               si.product_id AS "ProductId", p.sku AS "ProductSku", p.name AS "ProductName",
               u.code AS "UnitOfMeasureCode", si.quantity AS "Quantity"
        FROM inventory.stock_items si
        INNER JOIN inventory.warehouses w ON w.id = si.warehouse_id
        INNER JOIN catalog.products p ON p.id = si.product_id
        INNER JOIN catalog.units_of_measure u ON u.id = p.unit_of_measure_id
        """;

    public async Task<IReadOnlyCollection<StockItemDto>> GetListAsync(
        Guid? warehouseId,
        Guid? productId,
        CancellationToken cancellationToken)
    {
        var sql = new StringBuilder(BaseSql);
        var parameters = new DynamicParameters();
        var hasWhere = false;

        if (warehouseId is not null)
        {
            sql.Append(" WHERE si.warehouse_id = @WarehouseId");
            parameters.Add("WarehouseId", warehouseId);
            hasWhere = true;
        }

        if (productId is not null)
        {
            sql.Append(hasWhere ? " AND " : " WHERE ");
            sql.Append("si.product_id = @ProductId");
            parameters.Add("ProductId", productId);
        }

        sql.Append(" ORDER BY w.name, p.name");

        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql.ToString(), parameters, cancellationToken: cancellationToken);

        var result = await connection.QueryAsync<StockItemDto>(command);

        return result.ToList();
    }

    public async Task<IReadOnlyCollection<LowStockItemDto>> GetLowStockItemsAsync(int limit, CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT si.warehouse_id AS "WarehouseId", w.code AS "WarehouseCode", w.name AS "WarehouseName",
                   si.product_id AS "ProductId", p.sku AS "ProductSku", p.name AS "ProductName",
                   u.code AS "UnitOfMeasureCode", si.quantity AS "Quantity", p.min_stock_quantity AS "MinStockQuantity"
            FROM inventory.stock_items si
            INNER JOIN inventory.warehouses w ON w.id = si.warehouse_id
            INNER JOIN catalog.products p ON p.id = si.product_id
            INNER JOIN catalog.units_of_measure u ON u.id = p.unit_of_measure_id
            WHERE si.quantity <= p.min_stock_quantity
            ORDER BY (p.min_stock_quantity - si.quantity) DESC
            LIMIT @Limit
            """;

        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new { Limit = limit }, cancellationToken: cancellationToken);

        var result = await connection.QueryAsync<LowStockItemDto>(command);

        return result.ToList();
    }
}
