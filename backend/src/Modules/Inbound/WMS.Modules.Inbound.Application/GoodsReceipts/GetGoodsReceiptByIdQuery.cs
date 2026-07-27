using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Inbound.Application.Dtos;

namespace WMS.Modules.Inbound.Application.GoodsReceipts;

public sealed record GetGoodsReceiptByIdQuery(Guid Id) : IQuery<GoodsReceiptDto>;
