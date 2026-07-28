using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Outbound.Application.Abstractions;
using WMS.SharedKernel;

namespace WMS.Modules.Outbound.Application.GoodsIssues;

public sealed class ApproveGoodsIssueCommandHandler(IGoodsIssueWriteRepository writeRepository)
    : ICommandHandler<ApproveGoodsIssueCommand>
{
    public async Task<Result> Handle(ApproveGoodsIssueCommand request, CancellationToken cancellationToken)
    {
        var goodsIssue = await writeRepository.GetByIdAsync(request.Id, cancellationToken);

        if (goodsIssue is null)
        {
            return Result.Failure(Error.NotFound("GoodsIssue.NotFound", "The goods issue was not found."));
        }

        var approveResult = goodsIssue.Approve();

        if (approveResult.IsFailure)
        {
            return approveResult;
        }

        await writeRepository.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
