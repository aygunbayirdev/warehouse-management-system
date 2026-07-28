using System.Text;
using Dapper;
using WMS.BuildingBlocks.Infrastructure.Persistence;
using WMS.Modules.Transfer.Application.Abstractions;
using WMS.Modules.Transfer.Application.Dtos;
using WMS.Modules.Transfer.Domain;

namespace WMS.Modules.Transfer.Infrastructure.Repositories;

/// <summary>
/// Reporting query joining transfer.stock_transfers/stock_transfer_lines with catalog.products and
/// inventory.warehouses (twice, for source and destination) across schemas — the pragmatic
/// Dapper-only exception documented in CLAUDE.md's CQRS section.
/// </summary>
internal sealed class StockTransferReadRepository(ISqlConnectionFactory connectionFactory) : IStockTransferReadRepository
{
    private const string BaseSql = """
        SELECT st.id AS "Id",
               st.source_warehouse_id AS "SourceWarehouseId", sw.name AS "SourceWarehouseName",
               st.destination_warehouse_id AS "DestinationWarehouseId", dw.name AS "DestinationWarehouseName",
               st.status AS "Status", st.created_by_user_id AS "CreatedByUserId",
               st.created_at_utc AS "CreatedAtUtc", st.shipped_at_utc AS "ShippedAtUtc", st.received_at_utc AS "ReceivedAtUtc",
               stl.product_id AS "ProductId", p.sku AS "ProductSku", p.name AS "ProductName",
               stl.quantity AS "Quantity"
        FROM transfer.stock_transfers st
        INNER JOIN inventory.warehouses sw ON sw.id = st.source_warehouse_id
        INNER JOIN inventory.warehouses dw ON dw.id = st.destination_warehouse_id
        LEFT JOIN transfer.stock_transfer_lines stl ON stl.stock_transfer_id = st.id
        LEFT JOIN catalog.products p ON p.id = stl.product_id
        """;

    public async Task<StockTransferDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var sql = $"{BaseSql} WHERE st.id = @Id";

        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken);

        var rows = (await connection.QueryAsync<StockTransferRow>(command)).ToList();

        return rows.Count == 0 ? null : MapToDto(rows);
    }

    public async Task<IReadOnlyCollection<StockTransferDto>> GetListAsync(
        Guid? sourceWarehouseId,
        Guid? destinationWarehouseId,
        StockTransferStatus? status,
        CancellationToken cancellationToken)
    {
        var sql = new StringBuilder(BaseSql);
        var parameters = new DynamicParameters();
        var hasWhere = false;

        if (sourceWarehouseId is not null)
        {
            sql.Append(" WHERE st.source_warehouse_id = @SourceWarehouseId");
            parameters.Add("SourceWarehouseId", sourceWarehouseId);
            hasWhere = true;
        }

        if (destinationWarehouseId is not null)
        {
            sql.Append(hasWhere ? " AND " : " WHERE ");
            sql.Append("st.destination_warehouse_id = @DestinationWarehouseId");
            parameters.Add("DestinationWarehouseId", destinationWarehouseId);
            hasWhere = true;
        }

        if (status is not null)
        {
            sql.Append(hasWhere ? " AND " : " WHERE ");
            sql.Append("st.status = @Status");
            parameters.Add("Status", status.Value.ToString());
        }

        sql.Append(" ORDER BY st.created_at_utc DESC");

        using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(sql.ToString(), parameters, cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync<StockTransferRow>(command);

        return rows
            .GroupBy(row => row.Id)
            .Select(group => MapToDto(group.ToList()))
            .ToList();
    }

    private static StockTransferDto MapToDto(IReadOnlyCollection<StockTransferRow> rows)
    {
        var first = rows.First();

        var lines = rows
            .Where(row => row.ProductId is not null)
            .Select(row => new StockTransferLineDto(row.ProductId!.Value, row.ProductSku!, row.ProductName!, row.Quantity!.Value))
            .ToList();

        return new StockTransferDto(
            first.Id,
            first.SourceWarehouseId,
            first.SourceWarehouseName,
            first.DestinationWarehouseId,
            first.DestinationWarehouseName,
            first.Status,
            first.CreatedByUserId,
            first.CreatedAtUtc,
            first.ShippedAtUtc,
            first.ReceivedAtUtc,
            lines);
    }

    private sealed record StockTransferRow(
        Guid Id,
        Guid SourceWarehouseId,
        string SourceWarehouseName,
        Guid DestinationWarehouseId,
        string DestinationWarehouseName,
        string Status,
        Guid CreatedByUserId,
        DateTime CreatedAtUtc,
        DateTime? ShippedAtUtc,
        DateTime? ReceivedAtUtc,
        Guid? ProductId,
        string? ProductSku,
        string? ProductName,
        decimal? Quantity);
}
