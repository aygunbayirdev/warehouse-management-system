namespace WMS.Modules.Inbound.Application.Dtos;

public sealed record GoodsReceiptLineDto(Guid ProductId, string ProductSku, string ProductName, decimal Quantity);
