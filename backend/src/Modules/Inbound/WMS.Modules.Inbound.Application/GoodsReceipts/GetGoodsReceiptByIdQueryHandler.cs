using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Inbound.Application.Abstractions;
using WMS.Modules.Inbound.Application.Dtos;
using WMS.SharedKernel;

namespace WMS.Modules.Inbound.Application.GoodsReceipts;

public sealed class GetGoodsReceiptByIdQueryHandler(IGoodsReceiptReadRepository readRepository)
    : IQueryHandler<GetGoodsReceiptByIdQuery, GoodsReceiptDto>
{
    public async Task<Result<GoodsReceiptDto>> Handle(GetGoodsReceiptByIdQuery request, CancellationToken cancellationToken)
    {
        var goodsReceipt = await readRepository.GetByIdAsync(request.Id, cancellationToken);

        if (goodsReceipt is null)
        {
            return Result.Failure<GoodsReceiptDto>(Error.NotFound("GoodsReceipt.NotFound", "The goods receipt was not found."));
        }

        return goodsReceipt;
    }
}
