using WMS.BuildingBlocks.Application.Messaging;

namespace WMS.Modules.Catalog.Application.Products;

public sealed record DeleteProductCommand(Guid Id) : ICommand;
