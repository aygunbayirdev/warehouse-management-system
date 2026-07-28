namespace WMS.Modules.Inventory.Application.Dtos;

public sealed record StockMovementDto(
    Guid Id,
    Guid WarehouseId,
    string WarehouseCode,
    string WarehouseName,
    Guid ProductId,
    string ProductSku,
    string ProductName,
    string Type,
    decimal Quantity,
    string Reason,
    DateTime OccurredAtUtc);
