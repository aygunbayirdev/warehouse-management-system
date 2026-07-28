using System.Text;
using Dapper;
using WMS.BuildingBlocks.Infrastructure.Persistence;
using WMS.Modules.Outbound.Application.Abstractions;
using WMS.Modules.Outbound.Application.Dtos;
using WMS.Modules.Outbound.Domain;

namespace WMS.Modules.Outbound.Infrastructure.Repositories;

/// <summary>
/// Reporting query joining outbound.goods_issues/goods_issue_lines with catalog.products and
/// inventory.warehouses across schemas — the pragmatic Dapper-only exception documented in
/// CLAUDE.md's CQRS section.
/// </summary>
internal sealed class GoodsIssueReadRepository(ISqlConnectionFactory connectionFactory) : IGoodsIssueReadRepository
{
    private const string BaseSql = """
        SELECT gi.id AS "Id", gi.warehouse_id AS "WarehouseId", w.name AS "WarehouseName",
               gi.destination AS "Destination", gi.status AS "Status", gi.created_by_user_id AS "CreatedByUserId",
               gi.created_at_utc AS "CreatedAtUtc", gi.approved_at_utc AS "ApprovedAtUtc",
               gil.product_id AS "ProductId", p.sku AS "ProductSku", p.name AS "ProductName",
               gil.quantity AS "Quantity"
        FROM outbound.goods_issues gi
        INNER JOIN inventory.warehouses w ON w.id = gi.warehouse_id
        LEFT JOIN outbound.goods_issue_lines gil ON gil.goods_issue_id = gi.id
        LEFT JOIN catalog.products p ON p.id = gil.product_id
        """;

    public async Task<GoodsIssueDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var sql = $"{BaseSql} WHERE gi.id = @Id";

        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken);

        var rows = (await connection.QueryAsync<GoodsIssueRow>(command)).ToList();

        return rows.Count == 0 ? null : MapToDto(rows);
    }

    public async Task<IReadOnlyCollection<GoodsIssueDto>> GetListAsync(
        Guid? warehouseId,
        GoodsIssueStatus? status,
        CancellationToken cancellationToken)
    {
        var sql = new StringBuilder(BaseSql);
        var parameters = new DynamicParameters();
        var hasWhere = false;

        if (warehouseId is not null)
        {
            sql.Append(" WHERE gi.warehouse_id = @WarehouseId");
            parameters.Add("WarehouseId", warehouseId);
            hasWhere = true;
        }

        if (status is not null)
        {
            sql.Append(hasWhere ? " AND " : " WHERE ");
            sql.Append("gi.status = @Status");
            parameters.Add("Status", status.Value.ToString());
        }

        sql.Append(" ORDER BY gi.created_at_utc DESC");

        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql.ToString(), parameters, cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync<GoodsIssueRow>(command);

        return rows
            .GroupBy(row => row.Id)
            .Select(group => MapToDto(group.ToList()))
            .ToList();
    }

    private static GoodsIssueDto MapToDto(IReadOnlyCollection<GoodsIssueRow> rows)
    {
        var first = rows.First();

        var lines = rows
            .Where(row => row.ProductId is not null)
            .Select(row => new GoodsIssueLineDto(row.ProductId!.Value, row.ProductSku!, row.ProductName!, row.Quantity!.Value))
            .ToList();

        return new GoodsIssueDto(
            first.Id,
            first.WarehouseId,
            first.WarehouseName,
            first.Destination,
            first.Status,
            first.CreatedByUserId,
            first.CreatedAtUtc,
            first.ApprovedAtUtc,
            lines);
    }

    private sealed record GoodsIssueRow(
        Guid Id,
        Guid WarehouseId,
        string WarehouseName,
        string Destination,
        string Status,
        Guid CreatedByUserId,
        DateTime CreatedAtUtc,
        DateTime? ApprovedAtUtc,
        Guid? ProductId,
        string? ProductSku,
        string? ProductName,
        decimal? Quantity);
}
