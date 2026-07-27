using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Catalog.Application.Abstractions;
using WMS.SharedKernel;

namespace WMS.Modules.Catalog.Application.UnitsOfMeasure;

public sealed class DeleteUnitOfMeasureCommandHandler(
    IUnitOfMeasureWriteRepository writeRepository,
    IProductWriteRepository productWriteRepository)
    : ICommandHandler<DeleteUnitOfMeasureCommand>
{
    public async Task<Result> Handle(DeleteUnitOfMeasureCommand request, CancellationToken cancellationToken)
    {
        var unitOfMeasure = await writeRepository.GetByIdAsync(request.Id, cancellationToken);

        if (unitOfMeasure is null)
        {
            return Result.Failure(Error.NotFound("UnitOfMeasure.NotFound", "The unit of measure was not found."));
        }

        var isInUse = await productWriteRepository.ExistsWithUnitOfMeasureIdAsync(request.Id, cancellationToken);

        if (isInUse)
        {
            return Result.Failure(
                Error.Conflict("UnitOfMeasure.InUse", "The unit of measure is used by at least one product and cannot be deleted."));
        }

        writeRepository.Remove(unitOfMeasure);
        await writeRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
