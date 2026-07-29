using WMS.BuildingBlocks.Application.Messaging;
using WMS.BuildingBlocks.Application.Models;
using WMS.Modules.Catalog.Application.Abstractions;
using WMS.Modules.Catalog.Application.Dtos;
using WMS.SharedKernel;

namespace WMS.Modules.Catalog.Application.Products;

public sealed class GetProductsQueryHandler(IProductReadRepository readRepository)
    : IQueryHandler<GetProductsQuery, PagedResult<ProductDto>>
{
    public async Task<Result<PagedResult<ProductDto>>> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        var products = await readRepository.GetListAsync(
            request.CategoryId,
            request.Search,
            request.Page,
            request.PageSize,
            cancellationToken);

        return Result.Success(products);
    }
}
