namespace WMS.Modules.StockCount.Application.Dtos;

public sealed record StockCountDto(
    Guid Id,
    Guid WarehouseId,
    string WarehouseName,
    string Status,
    Guid CreatedByUserId,
    DateTime CreatedAtUtc,
    DateTime? ClosedAtUtc,
    IReadOnlyCollection<StockCountLineDto> Lines);
