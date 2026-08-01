using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMS.Api.Extensions;
using WMS.Modules.Identity.Domain;
using WMS.Modules.Transfer.Application.StockTransfers;
using WMS.Modules.Transfer.Domain;

namespace WMS.Api.Controllers;

[ApiController]
[Route("api/stock-transfers")]
[Authorize]
public sealed class StockTransfersController(ISender sender) : ControllerBase
{
    private const string ShipReceiveRoles = $"{RoleNames.Admin},{RoleNames.WarehouseManager},{RoleNames.WarehouseSupervisor}";

    [HttpGet]
    public async Task<IActionResult> GetStockTransfers(
        [FromQuery] Guid? sourceWarehouseId,
        [FromQuery] Guid? destinationWarehouseId,
        [FromQuery] StockTransferStatus? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(
            new GetStockTransfersQuery(sourceWarehouseId, destinationWarehouseId, status, page, pageSize), cancellationToken);

        return result.ToActionResult();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetStockTransferByIdQuery(id), cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateStockTransferRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateStockTransferCommand(
            request.SourceWarehouseId,
            request.DestinationWarehouseId,
            request.Lines,
            User.GetUserId());

        var result = await sender.Send(command, cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost("{id:guid}/ship")]
    [Authorize(Roles = ShipReceiveRoles)]
    public async Task<IActionResult> Ship(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ShipStockTransferCommand(id), cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost("{id:guid}/receive")]
    [Authorize(Roles = ShipReceiveRoles)]
    public async Task<IActionResult> Receive(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ReceiveStockTransferCommand(id), cancellationToken);

        return result.ToActionResult();
    }
}

public sealed record CreateStockTransferRequest(
    Guid SourceWarehouseId,
    Guid DestinationWarehouseId,
    IReadOnlyCollection<StockTransferLineInput> Lines);
