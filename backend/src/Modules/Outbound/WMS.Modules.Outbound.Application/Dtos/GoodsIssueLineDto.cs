namespace WMS.Modules.Outbound.Application.Dtos;

public sealed record GoodsIssueLineDto(Guid ProductId, string ProductSku, string ProductName, decimal Quantity);
