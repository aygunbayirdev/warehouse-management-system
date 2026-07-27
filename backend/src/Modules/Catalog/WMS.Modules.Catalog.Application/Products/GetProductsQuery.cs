using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Catalog.Application.Dtos;

namespace WMS.Modules.Catalog.Application.Products;

public sealed record GetProductsQuery(Guid? CategoryId, string? Search) : IQuery<IReadOnlyCollection<ProductDto>>;
