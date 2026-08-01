using WMS.BuildingBlocks.Application.Messaging;
using WMS.BuildingBlocks.Application.Models;
using WMS.Modules.Outbound.Application.Dtos;
using WMS.Modules.Outbound.Domain;

namespace WMS.Modules.Outbound.Application.GoodsIssues;

public sealed record GetGoodsIssuesQuery(Guid? WarehouseId, GoodsIssueStatus? Status, int Page, int PageSize)
    : IQuery<PagedResult<GoodsIssueDto>>;
