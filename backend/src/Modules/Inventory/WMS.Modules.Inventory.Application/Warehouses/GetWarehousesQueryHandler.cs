using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Inventory.Application.Abstractions;
using WMS.Modules.Inventory.Application.Dtos;
using WMS.SharedKernel;

namespace WMS.Modules.Inventory.Application.Warehouses;

public sealed class GetWarehousesQueryHandler(IWarehouseReadRepository readRepository)
    : IQueryHandler<GetWarehousesQuery, IReadOnlyCollection<WarehouseDto>>
{
    public async Task<Result<IReadOnlyCollection<WarehouseDto>>> Handle(
        GetWarehousesQuery request,
        CancellationToken cancellationToken)
    {
        var warehouses = await readRepository.GetAllAsync(cancellationToken);

        return Result.Success(warehouses);
    }
}
