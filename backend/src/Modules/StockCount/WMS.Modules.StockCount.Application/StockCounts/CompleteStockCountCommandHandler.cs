using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.StockCount.Application.Abstractions;
using WMS.Modules.StockCount.Domain;
using WMS.SharedKernel;

namespace WMS.Modules.StockCount.Application.StockCounts;

public sealed class CompleteStockCountCommandHandler(
    IStockCountWriteRepository stockCountWriteRepository,
    IStockCountAdjustmentWriteRepository stockCountAdjustmentWriteRepository)
    : ICommandHandler<CompleteStockCountCommand>
{
    public async Task<Result> Handle(CompleteStockCountCommand request, CancellationToken cancellationToken)
    {
        var stockCount = await stockCountWriteRepository.GetByIdAsync(request.Id, cancellationToken);

        if (stockCount is null)
        {
            return Result.Failure(Error.NotFound("StockCount.NotFound", "The stock count was not found."));
        }

        var completeResult = stockCount.Complete();

        if (completeResult.IsFailure)
        {
            return completeResult;
        }

        foreach (var line in stockCount.Lines.Where(line => line.Difference != 0))
        {
            var adjustment = StockCountAdjustment.Create(
                stockCount.Id,
                line.Id,
                stockCount.WarehouseId,
                line.ProductId,
                line.Difference);

            stockCountAdjustmentWriteRepository.Add(adjustment);
        }

        await stockCountWriteRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
