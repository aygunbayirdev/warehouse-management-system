using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Catalog.Application.Abstractions;
using WMS.Modules.Catalog.Domain;
using WMS.SharedKernel;

namespace WMS.Modules.Catalog.Application.UnitsOfMeasure;

public sealed class CreateUnitOfMeasureCommandHandler(IUnitOfMeasureWriteRepository writeRepository)
    : ICommandHandler<CreateUnitOfMeasureCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateUnitOfMeasureCommand request, CancellationToken cancellationToken)
    {
        var existing = await writeRepository.GetByCodeAsync(request.Code, cancellationToken);

        if (existing is not null)
        {
            return Result.Failure<Guid>(
                Error.Conflict("UnitOfMeasure.CodeAlreadyExists", $"A unit of measure with code '{request.Code}' already exists."));
        }

        var unitOfMeasure = UnitOfMeasure.Create(request.Code, request.Name);

        writeRepository.Add(unitOfMeasure);
        await writeRepository.SaveChangesAsync(cancellationToken);

        return unitOfMeasure.Id;
    }
}
