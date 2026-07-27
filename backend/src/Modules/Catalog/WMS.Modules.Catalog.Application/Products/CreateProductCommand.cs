using WMS.BuildingBlocks.Application.Messaging;

namespace WMS.Modules.Catalog.Application.Products;

public sealed record CreateProductCommand(
    string Sku,
    string Name,
    Guid UnitOfMeasureId,
    Guid? CategoryId,
    decimal MinStockQuantity) : ICommand<Guid>;
