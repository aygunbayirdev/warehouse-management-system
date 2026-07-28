using WMS.BuildingBlocks.Application.Messaging;

namespace WMS.Modules.StockCount.Application.StockCounts;

public sealed record SubmitStockCountLineCommand(Guid StockCountId, Guid ProductId, decimal CountedQuantity) : ICommand;
