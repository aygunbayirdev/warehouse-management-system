using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Outbound.Application.Abstractions;
using WMS.Modules.Outbound.Application.Dtos;
using WMS.SharedKernel;

namespace WMS.Modules.Outbound.Application.GoodsIssues;

public sealed class GetGoodsIssueByIdQueryHandler(IGoodsIssueReadRepository readRepository)
    : IQueryHandler<GetGoodsIssueByIdQuery, GoodsIssueDto>
{
    public async Task<Result<GoodsIssueDto>> Handle(GetGoodsIssueByIdQuery request, CancellationToken cancellationToken)
    {
        var goodsIssue = await readRepository.GetByIdAsync(request.Id, cancellationToken);

        if (goodsIssue is null)
        {
            return Result.Failure<GoodsIssueDto>(Error.NotFound("GoodsIssue.NotFound", "The goods issue was not found."));
        }

        return goodsIssue;
    }
}
