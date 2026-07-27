using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMS.Api.Extensions;
using WMS.Modules.Catalog.Application.Categories;
using WMS.Modules.Identity.Domain;

namespace WMS.Api.Controllers;

[ApiController]
[Route("api/categories")]
[Authorize]
public sealed class CategoriesController(ISender sender) : ControllerBase
{
    private const string ManageRoles = $"{RoleNames.Admin},{RoleNames.WarehouseManager}";

    [HttpGet]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCategoriesQuery(), cancellationToken);

        return result.ToActionResult();
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCategoryByIdQuery(id), cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Create(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.ToActionResult();
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Update(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateCategoryCommand(id, request.Name), cancellationToken);

        return result.ToActionResult();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = ManageRoles)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new DeleteCategoryCommand(id), cancellationToken);

        return result.ToActionResult();
    }
}

public sealed record UpdateCategoryRequest(string Name);
