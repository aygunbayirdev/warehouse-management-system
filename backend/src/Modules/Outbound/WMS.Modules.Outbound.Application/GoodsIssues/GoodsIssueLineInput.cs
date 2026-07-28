namespace WMS.Modules.Outbound.Application.GoodsIssues;

public sealed record GoodsIssueLineInput(Guid ProductId, decimal Quantity);
