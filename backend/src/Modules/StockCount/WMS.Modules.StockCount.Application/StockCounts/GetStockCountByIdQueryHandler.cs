using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.StockCount.Application.Abstractions;
using WMS.Modules.StockCount.Application.Dtos;
using WMS.SharedKernel;

namespace WMS.Modules.StockCount.Application.StockCounts;

public sealed class GetStockCountByIdQueryHandler(IStockCountReadRepository readRepository)
    : IQueryHandler<GetStockCountByIdQuery, StockCountDto>
{
    public async Task<Result<StockCountDto>> Handle(GetStockCountByIdQuery request, CancellationToken cancellationToken)
    {
        var stockCount = await readRepository.GetByIdAsync(request.Id, cancellationToken);

        if (stockCount is null)
        {
            return Result.Failure<StockCountDto>(Error.NotFound("StockCount.NotFound", "The stock count was not found."));
        }

        return stockCount;
    }
}
