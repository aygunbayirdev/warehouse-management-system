using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Inbound.Application.Abstractions;
using WMS.Modules.Inbound.Application.Dtos;
using WMS.SharedKernel;

namespace WMS.Modules.Inbound.Application.GoodsReceipts;

public sealed class GetGoodsReceiptsQueryHandler(IGoodsReceiptReadRepository readRepository)
    : IQueryHandler<GetGoodsReceiptsQuery, IReadOnlyCollection<GoodsReceiptDto>>
{
    public async Task<Result<IReadOnlyCollection<GoodsReceiptDto>>> Handle(
        GetGoodsReceiptsQuery request,
        CancellationToken cancellationToken)
    {
        var goodsReceipts = await readRepository.GetListAsync(request.WarehouseId, request.Status, cancellationToken);

        return Result.Success(goodsReceipts);
    }
}
