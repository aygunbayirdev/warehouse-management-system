using System.Text;
using Dapper;
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

    public async Task<IReadOnlyCollection<GoodsReceiptDto>> GetListAsync(
        Guid? warehouseId,
        GoodsReceiptStatus? status,
        CancellationToken cancellationToken)
    {
        var sql = new StringBuilder(BaseSql);
        var parameters = new DynamicParameters();
        var hasWhere = false;

        if (warehouseId is not null)
        {
            sql.Append(" WHERE gr.warehouse_id = @WarehouseId");
            parameters.Add("WarehouseId", warehouseId);
            hasWhere = true;
        }

        if (status is not null)
        {
            sql.Append(hasWhere ? " AND " : " WHERE ");
            sql.Append("gr.status = @Status");
            parameters.Add("Status", status.Value.ToString());
        }

        sql.Append(" ORDER BY gr.created_at_utc DESC");

        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql.ToString(), parameters, cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync<GoodsReceiptRow>(command);

        return rows
            .GroupBy(row => row.Id)
            .Select(group => MapToDto(group.ToList()))
            .ToList();
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
