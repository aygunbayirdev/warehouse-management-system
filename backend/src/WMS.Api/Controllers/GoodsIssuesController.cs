using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMS.Api.Extensions;
using WMS.Modules.Identity.Domain;
using WMS.Modules.Outbound.Application.GoodsIssues;
using WMS.Modules.Outbound.Domain;

namespace WMS.Api.Controllers;

[ApiController]
[Route("api/goods-issues")]
[Authorize]
public sealed class GoodsIssuesController(ISender sender) : ControllerBase
{
    private const string ApproveRoles = $"{RoleNames.Admin},{RoleNames.WarehouseManager},{RoleNames.WarehouseSupervisor}";

    [HttpGet]
    public async Task<IActionResult> GetGoodsIssues(
        [FromQuery] Guid? warehouseId,
        [FromQuery] GoodsIssueStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetGoodsIssuesQuery(warehouseId, status, page, pageSize), cancellationToken);

        return result.ToActionResult();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetGoodsIssueByIdQuery(id), cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateGoodsIssueRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateGoodsIssueCommand(request.WarehouseId, request.Destination, request.Lines, User.GetUserId());
        var result = await sender.Send(command, cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = ApproveRoles)]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ApproveGoodsIssueCommand(id), cancellationToken);

        return result.ToActionResult();
    }
}

public sealed record CreateGoodsIssueRequest(Guid WarehouseId, string Destination, IReadOnlyCollection<GoodsIssueLineInput> Lines);
