using WMS.BuildingBlocks.Application.Messaging;

namespace WMS.Modules.Outbound.Application.GoodsIssues;

public sealed record CreateGoodsIssueCommand(
    Guid WarehouseId,
    string Destination,
    IReadOnlyCollection<GoodsIssueLineInput> Lines,
    Guid CreatedByUserId) : ICommand<Guid>;
