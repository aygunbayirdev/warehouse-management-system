using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Inbound.Application.Dtos;
using WMS.Modules.Inbound.Domain;

namespace WMS.Modules.Inbound.Application.GoodsReceipts;

public sealed record GetGoodsReceiptsQuery(Guid? WarehouseId, GoodsReceiptStatus? Status)
    : IQuery<IReadOnlyCollection<GoodsReceiptDto>>;
