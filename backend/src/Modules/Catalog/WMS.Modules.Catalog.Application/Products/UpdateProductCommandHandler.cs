using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Catalog.Application.Abstractions;
using WMS.SharedKernel;

namespace WMS.Modules.Catalog.Application.Products;

public sealed class UpdateProductCommandHandler(
    IProductWriteRepository writeRepository,
    IUnitOfMeasureWriteRepository unitOfMeasureWriteRepository,
    ICategoryWriteRepository categoryWriteRepository)
    : ICommandHandler<UpdateProductCommand>
{
    public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await writeRepository.GetByIdAsync(request.Id, cancellationToken);

        if (product is null)
        {
            return Result.Failure(Error.NotFound("Product.NotFound", "The product was not found."));
        }

        var unitOfMeasure = await unitOfMeasureWriteRepository.GetByIdAsync(request.UnitOfMeasureId, cancellationToken);

        if (unitOfMeasure is null)
        {
            return Result.Failure(Error.NotFound("UnitOfMeasure.NotFound", "The unit of measure was not found."));
        }

        if (request.CategoryId is { } categoryId)
        {
            var category = await categoryWriteRepository.GetByIdAsync(categoryId, cancellationToken);

            if (category is null)
            {
                return Result.Failure(Error.NotFound("Category.NotFound", "The category was not found."));
            }
        }

        product.Update(request.Name, request.UnitOfMeasureId, request.CategoryId, request.MinStockQuantity);
        await writeRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
