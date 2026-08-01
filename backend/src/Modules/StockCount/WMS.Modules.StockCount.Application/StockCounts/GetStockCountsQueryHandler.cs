using WMS.BuildingBlocks.Application.Messaging;
using WMS.BuildingBlocks.Application.Models;
using WMS.Modules.StockCount.Application.Abstractions;
using WMS.Modules.StockCount.Application.Dtos;
using WMS.SharedKernel;

namespace WMS.Modules.StockCount.Application.StockCounts;

public sealed class GetStockCountsQueryHandler(IStockCountReadRepository readRepository)
    : IQueryHandler<GetStockCountsQuery, PagedResult<StockCountDto>>
{
    public async Task<Result<PagedResult<StockCountDto>>> Handle(
        GetStockCountsQuery request,
        CancellationToken cancellationToken)
    {
        var stockCounts = await readRepository.GetListAsync(
            request.WarehouseId, request.Status, request.Page, request.PageSize, cancellationToken);

        return Result.Success(stockCounts);
    }
}
