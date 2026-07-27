using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Inventory.Application.Abstractions;
using WMS.Modules.Inventory.Application.Dtos;
using WMS.SharedKernel;

namespace WMS.Modules.Inventory.Application.Warehouses;

public sealed class GetWarehouseByIdQueryHandler(IWarehouseReadRepository readRepository)
    : IQueryHandler<GetWarehouseByIdQuery, WarehouseDto>
{
    public async Task<Result<WarehouseDto>> Handle(GetWarehouseByIdQuery request, CancellationToken cancellationToken)
    {
        var warehouse = await readRepository.GetByIdAsync(request.Id, cancellationToken);

        if (warehouse is null)
        {
            return Result.Failure<WarehouseDto>(Error.NotFound("Warehouse.NotFound", "The warehouse was not found."));
        }

        return warehouse;
    }
}
