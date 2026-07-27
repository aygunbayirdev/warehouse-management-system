using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Catalog.Application.Abstractions;
using WMS.SharedKernel;

namespace WMS.Modules.Catalog.Application.Categories;

public sealed class DeleteCategoryCommandHandler(
    ICategoryWriteRepository writeRepository,
    IProductWriteRepository productWriteRepository)
    : ICommandHandler<DeleteCategoryCommand>
{
    public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
    {
        var category = await writeRepository.GetByIdAsync(request.Id, cancellationToken);

        if (category is null)
        {
            return Result.Failure(Error.NotFound("Category.NotFound", "The category was not found."));
        }

        var isInUse = await productWriteRepository.ExistsWithCategoryIdAsync(request.Id, cancellationToken);

        if (isInUse)
        {
            return Result.Failure(
                Error.Conflict("Category.InUse", "The category is used by at least one product and cannot be deleted."));
        }

        writeRepository.Remove(category);
        await writeRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
