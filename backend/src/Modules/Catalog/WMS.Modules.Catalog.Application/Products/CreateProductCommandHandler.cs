using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Catalog.Application.Abstractions;
using WMS.Modules.Catalog.Domain;
using WMS.SharedKernel;

namespace WMS.Modules.Catalog.Application.Products;

public sealed class CreateProductCommandHandler(
    IProductWriteRepository writeRepository,
    IUnitOfMeasureWriteRepository unitOfMeasureWriteRepository,
    ICategoryWriteRepository categoryWriteRepository)
    : ICommandHandler<CreateProductCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var existingSku = await writeRepository.GetBySkuAsync(request.Sku, cancellationToken);

        if (existingSku is not null)
        {
            return Result.Failure<Guid>(Error.Conflict("Product.SkuAlreadyExists", $"A product with SKU '{request.Sku}' already exists."));
        }

        var unitOfMeasure = await unitOfMeasureWriteRepository.GetByIdAsync(request.UnitOfMeasureId, cancellationToken);

        if (unitOfMeasure is null)
        {
            return Result.Failure<Guid>(Error.NotFound("UnitOfMeasure.NotFound", "The unit of measure was not found."));
        }

        if (request.CategoryId is { } categoryId)
        {
            var category = await categoryWriteRepository.GetByIdAsync(categoryId, cancellationToken);

            if (category is null)
            {
                return Result.Failure<Guid>(Error.NotFound("Category.NotFound", "The category was not found."));
            }
        }

        var product = Product.Create(request.Sku, request.Name, request.UnitOfMeasureId, request.CategoryId, request.MinStockQuantity);

        writeRepository.Add(product);
        await writeRepository.SaveChangesAsync(cancellationToken);

        return product.Id;
    }
}
