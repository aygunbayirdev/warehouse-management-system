using WMS.BuildingBlocks.Application.Messaging;
using WMS.BuildingBlocks.Application.Models;
using WMS.Modules.Inbound.Application.Dtos;
using WMS.Modules.Inbound.Domain;

namespace WMS.Modules.Inbound.Application.GoodsReceipts;

public sealed record GetGoodsReceiptsQuery(Guid? WarehouseId, GoodsReceiptStatus? Status, int Page, int PageSize)
    : IQuery<PagedResult<GoodsReceiptDto>>;
