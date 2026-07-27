using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Catalog.Application.Abstractions;
using WMS.Modules.Catalog.Domain;
using WMS.SharedKernel;

namespace WMS.Modules.Catalog.Application.Categories;

public sealed class CreateCategoryCommandHandler(ICategoryWriteRepository writeRepository)
    : ICommandHandler<CreateCategoryCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
    {
        var existing = await writeRepository.GetByNameAsync(request.Name, cancellationToken);

        if (existing is not null)
        {
            return Result.Failure<Guid>(
                Error.Conflict("Category.NameAlreadyExists", $"A category named '{request.Name}' already exists."));
        }

        var category = Category.Create(request.Name);

        writeRepository.Add(category);
        await writeRepository.SaveChangesAsync(cancellationToken);

        return category.Id;
    }
}
