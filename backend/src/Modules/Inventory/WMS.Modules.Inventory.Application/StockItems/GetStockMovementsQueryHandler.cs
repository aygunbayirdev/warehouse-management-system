using WMS.BuildingBlocks.Application.Messaging;
using WMS.BuildingBlocks.Application.Models;
using WMS.Modules.Inventory.Application.Abstractions;
using WMS.Modules.Inventory.Application.Dtos;
using WMS.SharedKernel;

namespace WMS.Modules.Inventory.Application.StockItems;

public sealed class GetStockMovementsQueryHandler(IStockMovementReadRepository readRepository)
    : IQueryHandler<GetStockMovementsQuery, PagedResult<StockMovementDto>>
{
    public async Task<Result<PagedResult<StockMovementDto>>> Handle(
        GetStockMovementsQuery request,
        CancellationToken cancellationToken)
    {
        var movements = await readRepository.GetListAsync(
            request.WarehouseId,
            request.ProductId,
            request.FromUtc,
            request.ToUtc,
            request.Page,
            request.PageSize,
            cancellationToken);

        return Result.Success(movements);
    }
}
