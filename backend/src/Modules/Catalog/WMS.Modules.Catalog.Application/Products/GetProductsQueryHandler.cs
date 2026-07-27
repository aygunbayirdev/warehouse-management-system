using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Catalog.Application.Abstractions;
using WMS.Modules.Catalog.Application.Dtos;
using WMS.SharedKernel;

namespace WMS.Modules.Catalog.Application.Products;

public sealed class GetProductsQueryHandler(IProductReadRepository readRepository)
    : IQueryHandler<GetProductsQuery, IReadOnlyCollection<ProductDto>>
{
    public async Task<Result<IReadOnlyCollection<ProductDto>>> Handle(
        GetProductsQuery request,
        CancellationToken cancellationToken)
    {
        var products = await readRepository.GetListAsync(request.CategoryId, request.Search, cancellationToken);

        return Result.Success(products);
    }
}
