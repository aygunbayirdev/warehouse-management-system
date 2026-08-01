using WMS.BuildingBlocks.Application.Messaging;
using WMS.BuildingBlocks.Application.Models;
using WMS.Modules.Inbound.Application.Abstractions;
using WMS.Modules.Inbound.Application.Dtos;
using WMS.SharedKernel;

namespace WMS.Modules.Inbound.Application.GoodsReceipts;

public sealed class GetGoodsReceiptsQueryHandler(IGoodsReceiptReadRepository readRepository)
    : IQueryHandler<GetGoodsReceiptsQuery, PagedResult<GoodsReceiptDto>>
{
    public async Task<Result<PagedResult<GoodsReceiptDto>>> Handle(
        GetGoodsReceiptsQuery request,
        CancellationToken cancellationToken)
    {
        var goodsReceipts = await readRepository.GetListAsync(
            request.WarehouseId, request.Status, request.Page, request.PageSize, cancellationToken);

        return Result.Success(goodsReceipts);
    }
}
