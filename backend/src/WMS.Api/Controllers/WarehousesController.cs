using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMS.Api.Extensions;
using WMS.Modules.Identity.Domain;
using WMS.Modules.Inventory.Application.Warehouses;

namespace WMS.Api.Controllers;

[ApiController]
[Route("api/warehouses")]
[Authorize]
public sealed class WarehousesController(ISender sender) : ControllerBase
{
    private const string ManageRoles = $"{RoleNames.Admin},{RoleNames.WarehouseManager}";

    [HttpGet]
    public async Task<IActionResult> GetWarehouses(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetWarehousesQuery(), cancellationToken);

        return result.ToActionResult();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetWarehouseByIdQuery(id), cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Create(CreateWarehouseCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.ToActionResult();
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Update(Guid id, UpdateWarehouseRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateWarehouseCommand(id, request.Name, request.Address), cancellationToken);

        return result.ToActionResult();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteWarehouseCommand(id), cancellationToken);

        return result.ToActionResult();
    }
}

public sealed record UpdateWarehouseRequest(string Name, string? Address);
