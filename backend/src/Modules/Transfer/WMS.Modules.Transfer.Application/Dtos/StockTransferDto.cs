namespace WMS.Modules.Transfer.Application.Dtos;

public sealed record StockTransferDto(
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
    IReadOnlyCollection<StockTransferLineDto> Lines);
