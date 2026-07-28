using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Transfer.Application.Abstractions;
using WMS.SharedKernel;

namespace WMS.Modules.Transfer.Application.StockTransfers;

public sealed class ReceiveStockTransferCommandHandler(IStockTransferWriteRepository writeRepository)
    : ICommandHandler<ReceiveStockTransferCommand>
{
    public async Task<Result> Handle(ReceiveStockTransferCommand request, CancellationToken cancellationToken)
    {
        var stockTransfer = await writeRepository.GetByIdAsync(request.Id, cancellationToken);

        if (stockTransfer is null)
        {
            return Result.Failure(Error.NotFound("StockTransfer.NotFound", "The stock transfer was not found."));
        }

        var receiveResult = stockTransfer.Receive();

        if (receiveResult.IsFailure)
        {
            return receiveResult;
        }

        await writeRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
