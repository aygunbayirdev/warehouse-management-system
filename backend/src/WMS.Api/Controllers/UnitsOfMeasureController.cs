using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMS.Api.Extensions;
using WMS.Modules.Catalog.Application.UnitsOfMeasure;
using WMS.Modules.Identity.Domain;

namespace WMS.Api.Controllers;

[ApiController]
[Route("api/units-of-measure")]
[Authorize]
public sealed class UnitsOfMeasureController(ISender sender) : ControllerBase
{
    private const string ManageRoles = $"{RoleNames.Admin},{RoleNames.WarehouseManager}";

    [HttpGet]
    public async Task<IActionResult> GetUnitsOfMeasure(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetUnitsOfMeasureQuery(), cancellationToken);

        return result.ToActionResult();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetUnitOfMeasureByIdQuery(id), cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Create(CreateUnitOfMeasureCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.ToActionResult();
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Update(Guid id, UpdateUnitOfMeasureRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateUnitOfMeasureCommand(id, request.Code, request.Name), cancellationToken);

        return result.ToActionResult();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteUnitOfMeasureCommand(id), cancellationToken);

        return result.ToActionResult();
    }
}

public sealed record UpdateUnitOfMeasureRequest(string Code, string Name);
