using System.Text;
using Dapper;
using WMS.BuildingBlocks.Application.Models;
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

    public async Task<PagedResult<GoodsIssueDto>> GetListAsync(
        Guid? warehouseId,
        GoodsIssueStatus? status,
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
            SELECT COUNT(*) FROM outbound.goods_issues gi{BuildWhere("gi")};
            {BaseSql} WHERE gi.id IN (
                SELECT gi2.id FROM outbound.goods_issues gi2{BuildWhere("gi2")}
                ORDER BY gi2.created_at_utc DESC
                LIMIT @PageSize OFFSET @Skip
            )
            ORDER BY gi.created_at_utc DESC;
            """;

        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, parameters, cancellationToken: cancellationToken);

        await using var multi = await connection.QueryMultipleAsync(command);
        var totalCount = await multi.ReadSingleAsync<int>();
        var rows = (await multi.ReadAsync<GoodsIssueRow>()).ToList();

        var items = rows
            .GroupBy(row => row.Id)
            .Select(group => MapToDto(group.ToList()))
            .ToList();

        return new PagedResult<GoodsIssueDto>(items, totalCount, page, pageSize);
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
