using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.StockCount.Application.Abstractions;
using WMS.Modules.StockCount.Application.Dtos;
using WMS.SharedKernel;

namespace WMS.Modules.StockCount.Application.StockCountAdjustments;

public sealed class GetStockCountAdjustmentsQueryHandler(IStockCountAdjustmentReadRepository readRepository)
    : IQueryHandler<GetStockCountAdjustmentsQuery, IReadOnlyCollection<StockCountAdjustmentDto>>
{
    public async Task<Result<IReadOnlyCollection<StockCountAdjustmentDto>>> Handle(
        GetStockCountAdjustmentsQuery request,
        CancellationToken cancellationToken)
    {
        var adjustments = await readRepository.GetListAsync(request.WarehouseId, request.Status, cancellationToken);

        return Result.Success(adjustments);
    }
}
