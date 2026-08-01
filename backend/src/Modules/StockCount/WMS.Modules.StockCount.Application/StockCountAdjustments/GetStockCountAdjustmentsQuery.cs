using WMS.BuildingBlocks.Application.Messaging;
using WMS.BuildingBlocks.Application.Models;
using WMS.Modules.StockCount.Application.Dtos;
using WMS.Modules.StockCount.Domain;

namespace WMS.Modules.StockCount.Application.StockCountAdjustments;

public sealed record GetStockCountAdjustmentsQuery(Guid? WarehouseId, StockCountAdjustmentStatus? Status, int Page, int PageSize)
    : IQuery<PagedResult<StockCountAdjustmentDto>>;
