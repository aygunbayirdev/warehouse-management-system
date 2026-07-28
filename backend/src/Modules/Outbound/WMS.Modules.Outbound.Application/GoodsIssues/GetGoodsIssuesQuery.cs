using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Outbound.Application.Dtos;
using WMS.Modules.Outbound.Domain;

namespace WMS.Modules.Outbound.Application.GoodsIssues;

public sealed record GetGoodsIssuesQuery(Guid? WarehouseId, GoodsIssueStatus? Status)
    : IQuery<IReadOnlyCollection<GoodsIssueDto>>;
