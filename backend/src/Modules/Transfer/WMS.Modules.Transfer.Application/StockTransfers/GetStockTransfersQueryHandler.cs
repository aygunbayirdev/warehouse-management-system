using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Transfer.Application.Abstractions;
using WMS.Modules.Transfer.Application.Dtos;
using WMS.SharedKernel;

namespace WMS.Modules.Transfer.Application.StockTransfers;

public sealed class GetStockTransfersQueryHandler(IStockTransferReadRepository readRepository)
    : IQueryHandler<GetStockTransfersQuery, IReadOnlyCollection<StockTransferDto>>
{
    public async Task<Result<IReadOnlyCollection<StockTransferDto>>> Handle(
        GetStockTransfersQuery request,
        CancellationToken cancellationToken)
    {
        var stockTransfers = await readRepository.GetListAsync(
            request.SourceWarehouseId,
            request.DestinationWarehouseId,
            request.Status,
            cancellationToken);

        return Result.Success(stockTransfers);
    }
}
