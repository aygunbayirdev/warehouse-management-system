using WMS.BuildingBlocks.Application.Messaging;

namespace WMS.Modules.Transfer.Application.StockTransfers;

public sealed record ReceiveStockTransferCommand(Guid Id) : ICommand;
