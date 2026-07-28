namespace WMS.Modules.Transfer.Application.StockTransfers;

public sealed record StockTransferLineInput(Guid ProductId, decimal Quantity);
