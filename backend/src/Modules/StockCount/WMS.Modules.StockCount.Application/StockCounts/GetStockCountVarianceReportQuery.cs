using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.StockCount.Application.Dtos;

namespace WMS.Modules.StockCount.Application.StockCounts;

public sealed record GetStockCountVarianceReportQuery(
    Guid? WarehouseId,
    DateTime? FromUtc,
    DateTime? ToUtc) : IQuery<IReadOnlyCollection<StockCountVarianceReportRowDto>>;
