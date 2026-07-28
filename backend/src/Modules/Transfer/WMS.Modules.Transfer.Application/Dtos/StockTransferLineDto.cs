namespace WMS.Modules.Transfer.Application.Dtos;

public sealed record StockTransferLineDto(Guid ProductId, string ProductSku, string ProductName, decimal Quantity);
