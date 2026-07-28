using WMS.BuildingBlocks.Application.Messaging;

namespace WMS.Modules.StockCount.Application.StockCountAdjustments;

public sealed record RejectStockCountAdjustmentCommand(Guid Id, Guid ApprovedByUserId) : ICommand;
