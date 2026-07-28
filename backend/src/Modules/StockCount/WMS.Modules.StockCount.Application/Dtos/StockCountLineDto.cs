namespace WMS.Modules.StockCount.Application.Dtos;

public sealed record StockCountLineDto(
    Guid ProductId,
    string ProductSku,
    string ProductName,
    decimal SystemQuantity,
    decimal CountedQuantity,
    decimal Difference);
