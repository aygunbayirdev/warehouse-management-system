using WMS.BuildingBlocks.Application.Messaging;

namespace WMS.Modules.Inbound.Application.GoodsReceipts;

public sealed record CreateGoodsReceiptCommand(
    Guid WarehouseId,
    IReadOnlyCollection<GoodsReceiptLineInput> Lines,
    Guid CreatedByUserId) : ICommand<Guid>;
