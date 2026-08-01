using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMS.Api.Extensions;
using WMS.Modules.Identity.Domain;
using WMS.Modules.Inventory.Application.StockItems;

namespace WMS.Api.Controllers;

/// <summary>
/// Stock level reporting plus a manual adjustment endpoint (initial stock load, corrections).
/// From Phase 5 on, the normal path for changing stock is Inbound/Outbound/Transfer/StockCount
/// modules sending IncreaseStockCommand/DecreaseStockCommand from their own domain-event handlers —
/// the endpoints here are for direct Admin use, not part of that flow.
/// </summary>
[ApiController]
[Route("api/stock")]
[Authorize]
public sealed class StockController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetStockLevels(
        [FromQuery] Guid? warehouseId,
        [FromQuery] Guid? productId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetStockItemsQuery(warehouseId, productId), cancellationToken);

        return result.ToActionResult();
    }

    [HttpGet("movements")]
    public async Task<IActionResult> GetStockMovements(
        [FromQuery] Guid? warehouseId,
        [FromQuery] Guid? productId,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new GetStockMovementsQuery(warehouseId, productId, fromUtc, toUtc, page, pageSize), cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost("increase")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Increase(IncreaseStockCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost("decrease")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<IActionResult> Decrease(DecreaseStockCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.ToActionResult();
    }
}
