namespace WMS.Modules.Inventory.Application.Dtos;

public sealed record LowStockItemDto(
    Guid WarehouseId,
    string WarehouseCode,
    string WarehouseName,
    Guid ProductId,
    string ProductSku,
    string ProductName,
    string UnitOfMeasureCode,
    decimal Quantity,
    decimal MinStockQuantity);
