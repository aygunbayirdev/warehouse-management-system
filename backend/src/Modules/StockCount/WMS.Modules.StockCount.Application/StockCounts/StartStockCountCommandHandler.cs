using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.StockCount.Application.Abstractions;
using WMS.SharedKernel;

namespace WMS.Modules.StockCount.Application.StockCounts;

public sealed class StartStockCountCommandHandler(IStockCountWriteRepository writeRepository)
    : ICommandHandler<StartStockCountCommand>
{
    public async Task<Result> Handle(StartStockCountCommand request, CancellationToken cancellationToken)
    {
        var stockCount = await writeRepository.GetByIdAsync(request.Id, cancellationToken);

        if (stockCount is null)
        {
            return Result.Failure(Error.NotFound("StockCount.NotFound", "The stock count was not found."));
        }

        var startResult = stockCount.Start();

        if (startResult.IsFailure)
        {
            return startResult;
        }

        await writeRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
