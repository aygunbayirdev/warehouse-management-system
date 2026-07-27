namespace WMS.Modules.Catalog.Application.Dtos;

public sealed record ProductDto(
    Guid Id,
    string Sku,
    string Name,
    Guid UnitOfMeasureId,
    string UnitOfMeasureCode,
    Guid? CategoryId,
    string? CategoryName,
    decimal MinStockQuantity);
