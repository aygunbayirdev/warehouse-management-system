using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMS.Api.Extensions;
using WMS.Modules.Identity.Domain;
using WMS.Modules.StockCount.Application.StockCountAdjustments;
using WMS.Modules.StockCount.Domain;

namespace WMS.Api.Controllers;

[ApiController]
[Route("api/stock-count-adjustments")]
[Authorize]
public sealed class StockCountAdjustmentsController(ISender sender) : ControllerBase
{
    private const string ApproveRoles = $"{RoleNames.Admin},{RoleNames.WarehouseManager}";

    [HttpGet]
    public async Task<IActionResult> GetStockCountAdjustments(
        [FromQuery] Guid? warehouseId,
        [FromQuery] StockCountAdjustmentStatus? status,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetStockCountAdjustmentsQuery(warehouseId, status), cancellationToken);

        return result.ToActionResult();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetStockCountAdjustmentByIdQuery(id), cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = ApproveRoles)]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ApproveStockCountAdjustmentCommand(id, User.GetUserId()), cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost("{id:guid}/reject")]
    [Authorize(Roles = ApproveRoles)]
    public async Task<IActionResult> Reject(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new RejectStockCountAdjustmentCommand(id, User.GetUserId()), cancellationToken);

        return result.ToActionResult();
    }
}
