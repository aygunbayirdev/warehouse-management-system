using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Outbound.Application.Abstractions;
using WMS.Modules.Outbound.Application.Dtos;
using WMS.SharedKernel;

namespace WMS.Modules.Outbound.Application.GoodsIssues;

public sealed class GetGoodsIssuesQueryHandler(IGoodsIssueReadRepository readRepository)
    : IQueryHandler<GetGoodsIssuesQuery, IReadOnlyCollection<GoodsIssueDto>>
{
    public async Task<Result<IReadOnlyCollection<GoodsIssueDto>>> Handle(
        GetGoodsIssuesQuery request,
        CancellationToken cancellationToken)
    {
        var goodsIssues = await readRepository.GetListAsync(request.WarehouseId, request.Status, cancellationToken);

        return Result.Success(goodsIssues);
    }
}
