using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMS.Api.Extensions;
using WMS.Modules.Identity.Domain;
using WMS.Modules.StockCount.Application.StockCounts;
using WMS.Modules.StockCount.Domain;

namespace WMS.Api.Controllers;

[ApiController]
[Route("api/stock-counts")]
[Authorize]
public sealed class StockCountsController(ISender sender) : ControllerBase
{
    private const string SessionManageRoles = $"{RoleNames.Admin},{RoleNames.WarehouseManager},{RoleNames.WarehouseSupervisor}";

    [HttpGet]
    public async Task<IActionResult> GetStockCounts(
        [FromQuery] Guid? warehouseId,
        [FromQuery] StockCountStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetStockCountsQuery(warehouseId, status, page, pageSize), cancellationToken);

        return result.ToActionResult();
    }

    [HttpGet("variance-report")]
    public async Task<IActionResult> GetVarianceReport(
        [FromQuery] Guid? warehouseId,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetStockCountVarianceReportQuery(warehouseId, fromUtc, toUtc), cancellationToken);

        return result.ToActionResult();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetStockCountByIdQuery(id), cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize(Roles = SessionManageRoles)]
    public async Task<IActionResult> Create(CreateStockCountRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateStockCountCommand(request.WarehouseId, User.GetUserId());
        var result = await sender.Send(command, cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost("{id:guid}/start")]
    [Authorize(Roles = SessionManageRoles)]
    public async Task<IActionResult> Start(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new StartStockCountCommand(id), cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost("{id:guid}/lines")]
    public async Task<IActionResult> SubmitLine(Guid id, SubmitStockCountLineRequest request, CancellationToken cancellationToken)
    {
        var command = new SubmitStockCountLineCommand(id, request.ProductId, request.CountedQuantity);
        var result = await sender.Send(command, cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost("{id:guid}/complete")]
    [Authorize(Roles = SessionManageRoles)]
    public async Task<IActionResult> Complete(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CompleteStockCountCommand(id), cancellationToken);

        return result.ToActionResult();
    }
}

public sealed record CreateStockCountRequest(Guid WarehouseId);

public sealed record SubmitStockCountLineRequest(Guid ProductId, decimal CountedQuantity);
