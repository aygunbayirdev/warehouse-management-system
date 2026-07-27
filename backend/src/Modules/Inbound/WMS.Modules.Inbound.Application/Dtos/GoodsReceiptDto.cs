namespace WMS.Modules.Inbound.Application.Dtos;

public sealed record GoodsReceiptDto(
    Guid Id,
    Guid WarehouseId,
    string WarehouseName,
    string Status,
    Guid CreatedByUserId,
    DateTime CreatedAtUtc,
    DateTime? ApprovedAtUtc,
    IReadOnlyCollection<GoodsReceiptLineDto> Lines);
