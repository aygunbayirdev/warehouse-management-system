using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Catalog.Application.Abstractions;
using WMS.Modules.Catalog.Application.Dtos;
using WMS.SharedKernel;

namespace WMS.Modules.Catalog.Application.Products;

public sealed class GetProductByIdQueryHandler(IProductReadRepository readRepository)
    : IQueryHandler<GetProductByIdQuery, ProductDto>
{
    public async Task<Result<ProductDto>> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await readRepository.GetByIdAsync(request.Id, cancellationToken);

        if (product is null)
        {
            return Result.Failure<ProductDto>(Error.NotFound("Product.NotFound", "The product was not found."));
        }

        return product;
    }
}
