using WMS.BuildingBlocks.Application.Messaging;
using WMS.BuildingBlocks.Application.Models;
using WMS.Modules.Catalog.Application.Dtos;

namespace WMS.Modules.Catalog.Application.Products;

public sealed record GetProductsQuery(Guid? CategoryId, string? Search, int Page, int PageSize)
    : IQuery<PagedResult<ProductDto>>;
