using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.StockCount.Application.Abstractions;
using WMS.SharedKernel;

namespace WMS.Modules.StockCount.Application.StockCountAdjustments;

public sealed class RejectStockCountAdjustmentCommandHandler(IStockCountAdjustmentWriteRepository writeRepository)
    : ICommandHandler<RejectStockCountAdjustmentCommand>
{
    public async Task<Result> Handle(RejectStockCountAdjustmentCommand request, CancellationToken cancellationToken)
    {
        var adjustment = await writeRepository.GetByIdAsync(request.Id, cancellationToken);

        if (adjustment is null)
        {
            return Result.Failure(Error.NotFound("StockCountAdjustment.NotFound", "The stock count adjustment was not found."));
        }

        var rejectResult = adjustment.Reject(request.ApprovedByUserId);

        if (rejectResult.IsFailure)
        {
            return rejectResult;
        }

        await writeRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
