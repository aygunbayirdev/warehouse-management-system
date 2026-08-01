using WMS.BuildingBlocks.Application.Messaging;
using WMS.BuildingBlocks.Application.Models;
using WMS.Modules.Transfer.Application.Abstractions;
using WMS.Modules.Transfer.Application.Dtos;
using WMS.SharedKernel;

namespace WMS.Modules.Transfer.Application.StockTransfers;

public sealed class GetStockTransfersQueryHandler(IStockTransferReadRepository readRepository)
    : IQueryHandler<GetStockTransfersQuery, PagedResult<StockTransferDto>>
{
    public async Task<Result<PagedResult<StockTransferDto>>> Handle(
        GetStockTransfersQuery request,
        CancellationToken cancellationToken)
    {
        var stockTransfers = await readRepository.GetListAsync(
            request.SourceWarehouseId,
            request.DestinationWarehouseId,
            request.Status,
            request.Page,
            request.PageSize,
            cancellationToken);

        return Result.Success(stockTransfers);
    }
}
