using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Inbound.Application.Abstractions;
using WMS.SharedKernel;

namespace WMS.Modules.Inbound.Application.GoodsReceipts;

public sealed class ApproveGoodsReceiptCommandHandler(IGoodsReceiptWriteRepository writeRepository)
    : ICommandHandler<ApproveGoodsReceiptCommand>
{
    public async Task<Result> Handle(ApproveGoodsReceiptCommand request, CancellationToken cancellationToken)
    {
        var goodsReceipt = await writeRepository.GetByIdAsync(request.Id, cancellationToken);

        if (goodsReceipt is null)
        {
            return Result.Failure(Error.NotFound("GoodsReceipt.NotFound", "The goods receipt was not found."));
        }

        var approveResult = goodsReceipt.Approve();

        if (approveResult.IsFailure)
        {
            return approveResult;
        }

        await writeRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
