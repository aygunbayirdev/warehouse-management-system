using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Catalog.Application.Abstractions;
using WMS.Modules.Catalog.Application.Dtos;
using WMS.SharedKernel;

namespace WMS.Modules.Catalog.Application.UnitsOfMeasure;

public sealed class GetUnitOfMeasureByIdQueryHandler(IUnitOfMeasureReadRepository readRepository)
    : IQueryHandler<GetUnitOfMeasureByIdQuery, UnitOfMeasureDto>
{
    public async Task<Result<UnitOfMeasureDto>> Handle(GetUnitOfMeasureByIdQuery request, CancellationToken cancellationToken)
    {
        var unitOfMeasure = await readRepository.GetByIdAsync(request.Id, cancellationToken);

        if (unitOfMeasure is null)
        {
            return Result.Failure<UnitOfMeasureDto>(Error.NotFound("UnitOfMeasure.NotFound", "The unit of measure was not found."));
        }

        return unitOfMeasure;
    }
}
