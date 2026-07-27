namespace WMS.Modules.Inbound.Application.GoodsReceipts;

public sealed record GoodsReceiptLineInput(Guid ProductId, decimal Quantity);
