using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Catalog.Application.Abstractions;
using WMS.SharedKernel;

namespace WMS.Modules.Catalog.Application.Categories;

public sealed class UpdateCategoryCommandHandler(ICategoryWriteRepository writeRepository)
    : ICommandHandler<UpdateCategoryCommand>
{
    public async Task<Result> Handle(UpdateCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await writeRepository.GetByIdAsync(request.Id, cancellationToken);

        if (category is null)
        {
            return Result.Failure(Error.NotFound("Category.NotFound", "The category was not found."));
        }

        var existing = await writeRepository.GetByNameAsync(request.Name, cancellationToken);

        if (existing is not null && existing.Id != request.Id)
        {
            return Result.Failure(
                Error.Conflict("Category.NameAlreadyExists", $"A category named '{request.Name}' already exists."));
        }

        category.Update(request.Name);
        await writeRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
