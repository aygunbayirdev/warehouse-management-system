using WMS.BuildingBlocks.Application.Messaging;
using WMS.BuildingBlocks.Application.Models;
using WMS.Modules.StockCount.Application.Abstractions;
using WMS.Modules.StockCount.Application.Dtos;
using WMS.SharedKernel;

namespace WMS.Modules.StockCount.Application.StockCountAdjustments;

public sealed class GetStockCountAdjustmentsQueryHandler(IStockCountAdjustmentReadRepository readRepository)
    : IQueryHandler<GetStockCountAdjustmentsQuery, PagedResult<StockCountAdjustmentDto>>
{
    public async Task<Result<PagedResult<StockCountAdjustmentDto>>> Handle(
        GetStockCountAdjustmentsQuery request,
        CancellationToken cancellationToken)
    {
        var adjustments = await readRepository.GetListAsync(
            request.WarehouseId, request.Status, request.Page, request.PageSize, cancellationToken);

        return Result.Success(adjustments);
    }
}
