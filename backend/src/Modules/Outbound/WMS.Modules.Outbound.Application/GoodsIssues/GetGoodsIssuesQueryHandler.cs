using WMS.BuildingBlocks.Application.Messaging;
using WMS.BuildingBlocks.Application.Models;
using WMS.Modules.Outbound.Application.Abstractions;
using WMS.Modules.Outbound.Application.Dtos;
using WMS.SharedKernel;

namespace WMS.Modules.Outbound.Application.GoodsIssues;

public sealed class GetGoodsIssuesQueryHandler(IGoodsIssueReadRepository readRepository)
    : IQueryHandler<GetGoodsIssuesQuery, PagedResult<GoodsIssueDto>>
{
    public async Task<Result<PagedResult<GoodsIssueDto>>> Handle(
        GetGoodsIssuesQuery request,
        CancellationToken cancellationToken)
    {
        var goodsIssues = await readRepository.GetListAsync(
            request.WarehouseId, request.Status, request.Page, request.PageSize, cancellationToken);

        return Result.Success(goodsIssues);
    }
}
