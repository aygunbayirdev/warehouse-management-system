using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.StockCount.Application.Abstractions;
using WMS.SharedKernel;

namespace WMS.Modules.StockCount.Application.StockCountAdjustments;

public sealed class ApproveStockCountAdjustmentCommandHandler(IStockCountAdjustmentWriteRepository writeRepository)
    : ICommandHandler<ApproveStockCountAdjustmentCommand>
{
    public async Task<Result> Handle(ApproveStockCountAdjustmentCommand request, CancellationToken cancellationToken)
    {
        var adjustment = await writeRepository.GetByIdAsync(request.Id, cancellationToken);

        if (adjustment is null)
        {
            return Result.Failure(Error.NotFound("StockCountAdjustment.NotFound", "The stock count adjustment was not found."));
        }

        var approveResult = adjustment.Approve(request.ApprovedByUserId);

        if (approveResult.IsFailure)
        {
            return approveResult;
        }

        await writeRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
