using WMS.BuildingBlocks.Application.Messaging;

namespace WMS.Modules.Transfer.Application.StockTransfers;

public sealed record CreateStockTransferCommand(
    Guid SourceWarehouseId,
    Guid DestinationWarehouseId,
    IReadOnlyCollection<StockTransferLineInput> Lines,
    Guid CreatedByUserId) : ICommand<Guid>;
