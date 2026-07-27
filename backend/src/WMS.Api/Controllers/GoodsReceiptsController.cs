using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMS.Api.Extensions;
using WMS.Modules.Identity.Domain;
using WMS.Modules.Inbound.Application.GoodsReceipts;
using WMS.Modules.Inbound.Domain;

namespace WMS.Api.Controllers;

[ApiController]
[Route("api/goods-receipts")]
[Authorize]
public sealed class GoodsReceiptsController(ISender sender) : ControllerBase
{
    private const string ApproveRoles = $"{RoleNames.Admin},{RoleNames.WarehouseManager},{RoleNames.WarehouseSupervisor}";

    [HttpGet]
    public async Task<IActionResult> GetGoodsReceipts(
        [FromQuery] Guid? warehouseId,
        [FromQuery] GoodsReceiptStatus? status,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetGoodsReceiptsQuery(warehouseId, status), cancellationToken);

        return result.ToActionResult();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetGoodsReceiptByIdQuery(id), cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateGoodsReceiptRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateGoodsReceiptCommand(request.WarehouseId, request.Lines, User.GetUserId());
        var result = await sender.Send(command, cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost("{id:guid}/approve")]
    [Authorize(Roles = ApproveRoles)]
    public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ApproveGoodsReceiptCommand(id), cancellationToken);

        return result.ToActionResult();
    }
}

public sealed record CreateGoodsReceiptRequest(Guid WarehouseId, IReadOnlyCollection<GoodsReceiptLineInput> Lines);
