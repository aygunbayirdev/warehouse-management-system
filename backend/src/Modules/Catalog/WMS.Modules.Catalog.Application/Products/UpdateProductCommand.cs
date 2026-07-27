using WMS.BuildingBlocks.Application.Messaging;

namespace WMS.Modules.Catalog.Application.Products;

public sealed record UpdateProductCommand(
    Guid Id,
    string Name,
    Guid UnitOfMeasureId,
    Guid? CategoryId,
    decimal MinStockQuantity) : ICommand;
