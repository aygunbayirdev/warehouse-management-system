namespace WMS.Modules.StockCount.Application.Dtos;

public sealed record StockCountAdjustmentDto(
    Guid Id,
    Guid StockCountId,
    Guid WarehouseId,
    string WarehouseName,
    Guid ProductId,
    string ProductSku,
    string ProductName,
    decimal DifferenceQuantity,
    string Status,
    Guid? ApprovedByUserId,
    DateTime CreatedAtUtc,
    DateTime? DecidedAtUtc);
