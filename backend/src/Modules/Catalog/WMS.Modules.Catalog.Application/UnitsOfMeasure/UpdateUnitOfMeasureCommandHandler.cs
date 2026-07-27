using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Catalog.Application.Abstractions;
using WMS.SharedKernel;

namespace WMS.Modules.Catalog.Application.UnitsOfMeasure;

public sealed class UpdateUnitOfMeasureCommandHandler(IUnitOfMeasureWriteRepository writeRepository)
    : ICommandHandler<UpdateUnitOfMeasureCommand>
{
    public async Task<Result> Handle(UpdateUnitOfMeasureCommand request, CancellationToken cancellationToken)
    {
        var unitOfMeasure = await writeRepository.GetByIdAsync(request.Id, cancellationToken);

        if (unitOfMeasure is null)
        {
            return Result.Failure(Error.NotFound("UnitOfMeasure.NotFound", "The unit of measure was not found."));
        }

        var existing = await writeRepository.GetByCodeAsync(request.Code, cancellationToken);

        if (existing is not null && existing.Id != request.Id)
        {
            return Result.Failure(
                Error.Conflict("UnitOfMeasure.CodeAlreadyExists", $"A unit of measure with code '{request.Code}' already exists."));
        }

        unitOfMeasure.Update(request.Code, request.Name);
        await writeRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
