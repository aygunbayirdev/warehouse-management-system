using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.StockCount.Application.Abstractions;
using WMS.Modules.StockCount.Application.Dtos;
using WMS.SharedKernel;

namespace WMS.Modules.StockCount.Application.StockCounts;

public sealed class GetStockCountsQueryHandler(IStockCountReadRepository readRepository)
    : IQueryHandler<GetStockCountsQuery, IReadOnlyCollection<StockCountDto>>
{
    public async Task<Result<IReadOnlyCollection<StockCountDto>>> Handle(
        GetStockCountsQuery request,
        CancellationToken cancellationToken)
    {
        var stockCounts = await readRepository.GetListAsync(request.WarehouseId, request.Status, cancellationToken);

        return Result.Success(stockCounts);
    }
}
