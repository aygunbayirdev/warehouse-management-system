using WMS.BuildingBlocks.Application.Messaging;

namespace WMS.Modules.Inbound.Application.GoodsReceipts;

public sealed record ApproveGoodsReceiptCommand(Guid Id) : ICommand;
