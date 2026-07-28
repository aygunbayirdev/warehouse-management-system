using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.StockCount.Application.Dtos;

namespace WMS.Modules.StockCount.Application.StockCounts;

public sealed record GetStockCountByIdQuery(Guid Id) : IQuery<StockCountDto>;
