using WMS.BuildingBlocks.Application.Messaging;

namespace WMS.Modules.Outbound.Application.GoodsIssues;

public sealed record ApproveGoodsIssueCommand(Guid Id) : ICommand;
