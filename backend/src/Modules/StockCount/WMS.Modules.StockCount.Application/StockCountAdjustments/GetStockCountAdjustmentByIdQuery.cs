using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.StockCount.Application.Dtos;

namespace WMS.Modules.StockCount.Application.StockCountAdjustments;

public sealed record GetStockCountAdjustmentByIdQuery(Guid Id) : IQuery<StockCountAdjustmentDto>;
