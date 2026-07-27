using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Catalog.Application.Abstractions;
using WMS.Modules.Catalog.Application.Dtos;
using WMS.SharedKernel;

namespace WMS.Modules.Catalog.Application.Categories;

public sealed class GetCategoriesQueryHandler(ICategoryReadRepository readRepository)
    : IQueryHandler<GetCategoriesQuery, IReadOnlyCollection<CategoryDto>>
{
    public async Task<Result<IReadOnlyCollection<CategoryDto>>> Handle(
        GetCategoriesQuery request,
        CancellationToken cancellationToken)
    {
        var categories = await readRepository.GetAllAsync(cancellationToken);

        return Result.Success(categories);
    }
}
