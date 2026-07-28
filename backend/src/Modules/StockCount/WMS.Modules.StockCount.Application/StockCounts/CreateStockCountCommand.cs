using WMS.BuildingBlocks.Application.Messaging;

namespace WMS.Modules.StockCount.Application.StockCounts;

public sealed record CreateStockCountCommand(Guid WarehouseId, Guid CreatedByUserId) : ICommand<Guid>;
