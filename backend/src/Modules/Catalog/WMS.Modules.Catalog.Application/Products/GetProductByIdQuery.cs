using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Catalog.Application.Dtos;

namespace WMS.Modules.Catalog.Application.Products;

public sealed record GetProductByIdQuery(Guid Id) : IQuery<ProductDto>;
