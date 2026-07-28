using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Transfer.Application.Abstractions;
using WMS.Modules.Transfer.Application.Dtos;
using WMS.SharedKernel;

namespace WMS.Modules.Transfer.Application.StockTransfers;

public sealed class GetStockTransferByIdQueryHandler(IStockTransferReadRepository readRepository)
    : IQueryHandler<GetStockTransferByIdQuery, StockTransferDto>
{
    public async Task<Result<StockTransferDto>> Handle(GetStockTransferByIdQuery request, CancellationToken cancellationToken)
    {
        var stockTransfer = await readRepository.GetByIdAsync(request.Id, cancellationToken);

        if (stockTransfer is null)
        {
            return Result.Failure<StockTransferDto>(Error.NotFound("StockTransfer.NotFound", "The stock transfer was not found."));
        }

        return stockTransfer;
    }
}
