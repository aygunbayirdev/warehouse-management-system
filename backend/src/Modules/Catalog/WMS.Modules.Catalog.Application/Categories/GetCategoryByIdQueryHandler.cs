using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Catalog.Application.Abstractions;
using WMS.Modules.Catalog.Application.Dtos;
using WMS.SharedKernel;

namespace WMS.Modules.Catalog.Application.Categories;

public sealed class GetCategoryByIdQueryHandler(ICategoryReadRepository readRepository)
    : IQueryHandler<GetCategoryByIdQuery, CategoryDto>
{
    public async Task<Result<CategoryDto>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await readRepository.GetByIdAsync(request.Id, cancellationToken);

        if (category is null)
        {
            return Result.Failure<CategoryDto>(Error.NotFound("Category.NotFound", "The category was not found."));
        }

        return category;
    }
}
