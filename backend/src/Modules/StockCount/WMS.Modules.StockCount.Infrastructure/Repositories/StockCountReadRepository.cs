using System.Text;
using Dapper;
using WMS.BuildingBlocks.Application.Models;
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

    public async Task<PagedResult<StockCountDto>> GetListAsync(
        Guid? warehouseId,
        StockCountStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var parameters = new DynamicParameters();
        parameters.Add("PageSize", pageSize);
        parameters.Add("Skip", (page - 1) * pageSize);

        if (warehouseId is not null)
        {
            parameters.Add("WarehouseId", warehouseId);
        }

        if (status is not null)
        {
            parameters.Add("Status", status.Value.ToString());
        }

        string BuildWhere(string alias)
        {
            var clause = new StringBuilder();
            var hasWhere = false;

            if (warehouseId is not null)
            {
                clause.Append($" WHERE {alias}.warehouse_id = @WarehouseId");
                hasWhere = true;
            }

            if (status is not null)
            {
                clause.Append(hasWhere ? " AND " : " WHERE ");
                clause.Append($"{alias}.status = @Status");
            }

            return clause.ToString();
        }

        // Pagination runs over a distinct header-id subquery, see GoodsReceiptReadRepository for why.
        var sql = $"""
            SELECT COUNT(*) FROM stockcount.stock_counts sc{BuildWhere("sc")};
            {BaseSql} WHERE sc.id IN (
                SELECT sc2.id FROM stockcount.stock_counts sc2{BuildWhere("sc2")}
                ORDER BY sc2.created_at_utc DESC
                LIMIT @PageSize OFFSET @Skip
            )
            ORDER BY sc.created_at_utc DESC;
            """;

        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

        await using var multi = await connection.QueryMultipleAsync(command);
        var totalCount = await multi.ReadSingleAsync<int>();
        var rows = (await multi.ReadAsync<StockCountRow>()).ToList();

        var items = rows
            .GroupBy(row => row.Id)
            .Select(group => MapToDto(group.ToList()))
            .ToList();

        return new PagedResult<StockCountDto>(items, totalCount, page, pageSize);
    }

    public async Task<IReadOnlyCollection<StockCountVarianceReportRowDto>> GetVarianceReportAsync(
        Guid? warehouseId,
        DateTime? fromUtc,
        DateTime? toUtc,
        CancellationToken cancellationToken)
    {
        var sql = new StringBuilder("""
            SELECT sc.id AS "StockCountId", sc.warehouse_id AS "WarehouseId", w.name AS "WarehouseName",
                   scl.product_id AS "ProductId", p.sku AS "ProductSku", p.name AS "ProductName",
                   scl.system_quantity AS "SystemQuantity", scl.counted_quantity AS "CountedQuantity",
                   scl.difference AS "Difference", sc.closed_at_utc AS "ClosedAtUtc"
            FROM stockcount.stock_counts sc
            INNER JOIN stockcount.stock_count_lines scl ON scl.stock_count_id = sc.id
            INNER JOIN inventory.warehouses w ON w.id = sc.warehouse_id
            INNER JOIN catalog.products p ON p.id = scl.product_id
            WHERE sc.status = 'Completed' AND scl.difference <> 0
            """);
        var parameters = new DynamicParameters();

        if (warehouseId is not null)
        {
            sql.Append(" AND sc.warehouse_id = @WarehouseId");
            parameters.Add("WarehouseId", warehouseId);
        }

        if (fromUtc is not null)
        {
            sql.Append(" AND sc.closed_at_utc >= @FromUtc");
            parameters.Add("FromUtc", fromUtc);
        }

        if (toUtc is not null)
        {
            sql.Append(" AND sc.closed_at_utc <= @ToUtc");
            parameters.Add("ToUtc", toUtc);
        }

        sql.Append(" ORDER BY sc.closed_at_utc DESC");

        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql.ToString(), parameters, cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync<StockCountVarianceReportRowDto>(command);

        return rows.ToList();
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
