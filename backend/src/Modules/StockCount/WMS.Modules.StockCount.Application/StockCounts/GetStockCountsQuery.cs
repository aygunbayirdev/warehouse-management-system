using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.StockCount.Application.Dtos;
using WMS.Modules.StockCount.Domain;

namespace WMS.Modules.StockCount.Application.StockCounts;

public sealed record GetStockCountsQuery(Guid? WarehouseId, StockCountStatus? Status) : IQuery<IReadOnlyCollection<StockCountDto>>;
