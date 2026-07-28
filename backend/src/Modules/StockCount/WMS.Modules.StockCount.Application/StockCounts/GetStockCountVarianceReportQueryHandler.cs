using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.StockCount.Application.Abstractions;
using WMS.Modules.StockCount.Application.Dtos;
using WMS.SharedKernel;

namespace WMS.Modules.StockCount.Application.StockCounts;

public sealed class GetStockCountVarianceReportQueryHandler(IStockCountReadRepository readRepository)
    : IQueryHandler<GetStockCountVarianceReportQuery, IReadOnlyCollection<StockCountVarianceReportRowDto>>
{
    public async Task<Result<IReadOnlyCollection<StockCountVarianceReportRowDto>>> Handle(
        GetStockCountVarianceReportQuery request,
        CancellationToken cancellationToken)
    {
        var rows = await readRepository.GetVarianceReportAsync(
            request.WarehouseId,
            request.FromUtc,
            request.ToUtc,
            cancellationToken);

        return Result.Success(rows);
    }
}
