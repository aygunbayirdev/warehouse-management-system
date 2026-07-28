using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Inventory.Application.Abstractions;
using WMS.Modules.Inventory.Application.Dtos;
using WMS.SharedKernel;

namespace WMS.Modules.Inventory.Application.StockItems;

public sealed class GetStockMovementsQueryHandler(IStockMovementReadRepository readRepository)
    : IQueryHandler<GetStockMovementsQuery, IReadOnlyCollection<StockMovementDto>>
{
    public async Task<Result<IReadOnlyCollection<StockMovementDto>>> Handle(
        GetStockMovementsQuery request,
        CancellationToken cancellationToken)
    {
        var movements = await readRepository.GetListAsync(
            request.WarehouseId,
            request.ProductId,
            request.FromUtc,
            request.ToUtc,
            cancellationToken);

        return Result.Success(movements);
    }
}
