using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Catalog.Application.Abstractions;
using WMS.SharedKernel;

namespace WMS.Modules.Catalog.Application.Products;

public sealed class DeleteProductCommandHandler(IProductWriteRepository writeRepository)
    : ICommandHandler<DeleteProductCommand>
{
    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await writeRepository.GetByIdAsync(request.Id, cancellationToken);

        if (product is null)
        {
            return Result.Failure(Error.NotFound("Product.NotFound", "The product was not found."));
        }

        writeRepository.Remove(product);
        await writeRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
