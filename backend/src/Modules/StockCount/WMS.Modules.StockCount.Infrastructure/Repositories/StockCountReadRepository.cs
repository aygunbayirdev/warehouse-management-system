using System.Text;
using Dapper;
using WMS.BuildingBlocks.Infrastructure.Persistence;
using WMS.Modules.StockCount.Application.Abstractions;
using WMS.Modules.StockCount.Application.Dtos;
using WMS.Modules.StockCount.Domain;

namespace WMS.Modules.StockCount.Infrastructure.Repositories;

/// <summary>
/// Reporting query joining stockcount.stock_counts/stock_count_lines with catalog.products and
/// inventory.warehouses across schemas — the pragmatic Dapper-only exception documented in
/// CLAUDE.md's CQRS section.
/// </summary>
internal sealed class StockCountReadRepository(ISqlConnectionFactory connectionFactory) : IStockCountReadRepository
{
    private const string BaseSql = """
        SELECT sc.id AS "Id", sc.warehouse_id AS "WarehouseId", w.name AS "WarehouseName",
               sc.status AS "Status", sc.created_by_user_id AS "CreatedByUserId",
               sc.created_at_utc AS "CreatedAtUtc", sc.closed_at_utc AS "ClosedAtUtc",
               scl.product_id AS "ProductId", p.sku AS "ProductSku", p.name AS "ProductName",
               scl.system_quantity AS "SystemQuantity", scl.counted_quantity AS "CountedQuantity", scl.difference AS "Difference"
        FROM stockcount.stock_counts sc
        INNER JOIN inventory.warehouses w ON w.id = sc.warehouse_id
        LEFT JOIN stockcount.stock_count_lines scl ON scl.stock_count_id = sc.id
        LEFT JOIN catalog.products p ON p.id = scl.product_id
        """;

    public async Task<StockCountDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var sql = $"{BaseSql} WHERE sc.id = @Id";

        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken);

        var rows = (await connection.QueryAsync<StockCountRow>(command)).ToList();

        return rows.Count == 0 ? null : MapToDto(rows);
    }

    public async Task<IReadOnlyCollection<StockCountDto>> GetListAsync(
        Guid? warehouseId,
        StockCountStatus? status,
        CancellationToken cancellationToken)
    {
        var sql = new StringBuilder(BaseSql);
        var parameters = new DynamicParameters();
        var hasWhere = false;

        if (warehouseId is not null)
        {
            sql.Append(" WHERE sc.warehouse_id = @WarehouseId");
            parameters.Add("WarehouseId", warehouseId);
            hasWhere = true;
        }

        if (status is not null)
        {
            sql.Append(hasWhere ? " AND " : " WHERE ");
            sql.Append("sc.status = @Status");
            parameters.Add("Status", status.Value.ToString());
        }

        sql.Append(" ORDER BY sc.created_at_utc DESC");

        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql.ToString(), parameters, cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync<StockCountRow>(command);

        return rows
            .GroupBy(row => row.Id)
            .Select(group => MapToDto(group.ToList()))
            .ToList();
    }

    private static StockCountDto MapToDto(IReadOnlyCollection<StockCountRow> rows)
    {
        var first = rows.First();

        var lines = rows
            .Where(row => row.ProductId is not null)
            .Select(row => new StockCountLineDto(
                row.ProductId!.Value,
                row.ProductSku!,
                row.ProductName!,
                row.SystemQuantity!.Value,
                row.CountedQuantity!.Value,
                row.Difference!.Value))
            .ToList();

        return new StockCountDto(
            first.Id,
            first.WarehouseId,
            first.WarehouseName,
            first.Status,
            first.CreatedByUserId,
            first.CreatedAtUtc,
            first.ClosedAtUtc,
            lines);
    }

    private sealed record StockCountRow(
        Guid Id,
        Guid WarehouseId,
        string WarehouseName,
        string Status,
        Guid CreatedByUserId,
        DateTime CreatedAtUtc,
        DateTime? ClosedAtUtc,
        Guid? ProductId,
        string? ProductSku,
        string? ProductName,
        decimal? SystemQuantity,
        decimal? CountedQuantity,
        decimal? Difference);
}
