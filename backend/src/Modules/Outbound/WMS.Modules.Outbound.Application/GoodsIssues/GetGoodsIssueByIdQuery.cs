using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Outbound.Application.Dtos;

namespace WMS.Modules.Outbound.Application.GoodsIssues;

public sealed record GetGoodsIssueByIdQuery(Guid Id) : IQuery<GoodsIssueDto>;
