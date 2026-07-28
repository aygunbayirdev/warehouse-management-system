using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.StockCount.Application.Abstractions;
using WMS.Modules.StockCount.Application.Dtos;
using WMS.SharedKernel;

namespace WMS.Modules.StockCount.Application.StockCountAdjustments;

public sealed class GetStockCountAdjustmentByIdQueryHandler(IStockCountAdjustmentReadRepository readRepository)
    : IQueryHandler<GetStockCountAdjustmentByIdQuery, StockCountAdjustmentDto>
{
    public async Task<Result<StockCountAdjustmentDto>> Handle(
        GetStockCountAdjustmentByIdQuery request,
        CancellationToken cancellationToken)
    {
        var adjustment = await readRepository.GetByIdAsync(request.Id, cancellationToken);

        if (adjustment is null)
        {
            return Result.Failure<StockCountAdjustmentDto>(
                Error.NotFound("StockCountAdjustment.NotFound", "The stock count adjustment was not found."));
        }

        return adjustment;
    }
}
