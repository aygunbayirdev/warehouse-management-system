using System.Text;
using Dapper;
using WMS.BuildingBlocks.Application.Models;
using WMS.BuildingBlocks.Infrastructure.Persistence;
using WMS.Modules.Inbound.Application.Abstractions;
using WMS.Modules.Inbound.Application.Dtos;
using WMS.Modules.Inbound.Domain;

namespace WMS.Modules.Inbound.Infrastructure.Repositories;

/// <summary>
/// Reporting query joining inbound.goods_receipts/goods_receipt_lines with catalog.products and
/// inventory.warehouses across schemas — the pragmatic Dapper-only exception documented in
/// CLAUDE.md's CQRS section.
/// </summary>
internal sealed class GoodsReceiptReadRepository(ISqlConnectionFactory connectionFactory) : IGoodsReceiptReadRepository
{
    private const string BaseSql = """
        SELECT gr.id AS "Id", gr.warehouse_id AS "WarehouseId", w.name AS "WarehouseName",
               gr.status AS "Status", gr.created_by_user_id AS "CreatedByUserId",
               gr.created_at_utc AS "CreatedAtUtc", gr.approved_at_utc AS "ApprovedAtUtc",
               grl.product_id AS "ProductId", p.sku AS "ProductSku", p.name AS "ProductName",
               grl.quantity AS "Quantity"
        FROM inbound.goods_receipts gr
        INNER JOIN inventory.warehouses w ON w.id = gr.warehouse_id
        LEFT JOIN inbound.goods_receipt_lines grl ON grl.goods_receipt_id = gr.id
        LEFT JOIN catalog.products p ON p.id = grl.product_id
        """;

    public async Task<GoodsReceiptDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var sql = $"{BaseSql} WHERE gr.id = @Id";

        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken);

        var rows = (await connection.QueryAsync<GoodsReceiptRow>(command)).ToList();

        return rows.Count == 0 ? null : MapToDto(rows);
    }

    public async Task<PagedResult<GoodsReceiptDto>> GetListAsync(
        Guid? warehouseId,
        GoodsReceiptStatus? status,
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

        // Pagination runs over a distinct header-id subquery (not the flat joined rows below) because
        // BaseSql fans out one row per line — LIMIT/OFFSET on the flat result would cut a receipt's
        // lines mid-way and make COUNT(*) count lines instead of receipts.
        var sql = $"""
            SELECT COUNT(*) FROM inbound.goods_receipts gr{BuildWhere("gr")};
            {BaseSql} WHERE gr.id IN (
                SELECT gr2.id FROM inbound.goods_receipts gr2{BuildWhere("gr2")}
                ORDER BY gr2.created_at_utc DESC
                LIMIT @PageSize OFFSET @Skip
            )
            ORDER BY gr.created_at_utc DESC;
            """;

        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

        await using var multi = await connection.QueryMultipleAsync(command);
        var totalCount = await multi.ReadSingleAsync<int>();
        var rows = (await multi.ReadAsync<GoodsReceiptRow>()).ToList();

        var items = rows
            .GroupBy(row => row.Id)
            .Select(group => MapToDto(group.ToList()))
            .ToList();

        return new PagedResult<GoodsReceiptDto>(items, totalCount, page, pageSize);
    }

    private static GoodsReceiptDto MapToDto(IReadOnlyCollection<GoodsReceiptRow> rows)
    {
        var first = rows.First();

        var lines = rows
            .Where(row => row.ProductId is not null)
            .Select(row => new GoodsReceiptLineDto(row.ProductId!.Value, row.ProductSku!, row.ProductName!, row.Quantity!.Value))
            .ToList();

        return new GoodsReceiptDto(
            first.Id,
            first.WarehouseId,
            first.WarehouseName,
            first.Status,
            first.CreatedByUserId,
            first.CreatedAtUtc,
            first.ApprovedAtUtc,
            lines);
    }

    private sealed record GoodsReceiptRow(
        Guid Id,
        Guid WarehouseId,
        string WarehouseName,
        string Status,
        Guid CreatedByUserId,
        DateTime CreatedAtUtc,
        DateTime? ApprovedAtUtc,
        Guid? ProductId,
        string? ProductSku,
        string? ProductName,
        decimal? Quantity);
}
