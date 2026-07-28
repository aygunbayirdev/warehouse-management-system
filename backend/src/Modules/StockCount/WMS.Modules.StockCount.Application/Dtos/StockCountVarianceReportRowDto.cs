namespace WMS.Modules.StockCount.Application.Dtos;

public sealed record StockCountVarianceReportRowDto(
    Guid StockCountId,
    Guid WarehouseId,
    string WarehouseName,
    Guid ProductId,
    string ProductSku,
    string ProductName,
    decimal SystemQuantity,
    decimal CountedQuantity,
    decimal Difference,
    DateTime ClosedAtUtc);
