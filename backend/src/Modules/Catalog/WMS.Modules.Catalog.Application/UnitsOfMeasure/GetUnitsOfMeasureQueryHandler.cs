using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Catalog.Application.Abstractions;
using WMS.Modules.Catalog.Application.Dtos;
using WMS.SharedKernel;

namespace WMS.Modules.Catalog.Application.UnitsOfMeasure;

public sealed class GetUnitsOfMeasureQueryHandler(IUnitOfMeasureReadRepository readRepository)
    : IQueryHandler<GetUnitsOfMeasureQuery, IReadOnlyCollection<UnitOfMeasureDto>>
{
    public async Task<Result<IReadOnlyCollection<UnitOfMeasureDto>>> Handle(
        GetUnitsOfMeasureQuery request,
        CancellationToken cancellationToken)
    {
        var unitsOfMeasure = await readRepository.GetAllAsync(cancellationToken);

        return Result.Success(unitsOfMeasure);
    }
}
