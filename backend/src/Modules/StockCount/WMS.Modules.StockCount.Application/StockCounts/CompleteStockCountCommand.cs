using WMS.BuildingBlocks.Application.Messaging;

namespace WMS.Modules.StockCount.Application.StockCounts;

public sealed record CompleteStockCountCommand(Guid Id) : ICommand;
