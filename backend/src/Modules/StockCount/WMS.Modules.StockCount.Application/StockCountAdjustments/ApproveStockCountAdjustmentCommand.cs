using WMS.BuildingBlocks.Application.Messaging;

namespace WMS.Modules.StockCount.Application.StockCountAdjustments;

public sealed record ApproveStockCountAdjustmentCommand(Guid Id, Guid ApprovedByUserId) : ICommand;
