using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WMS.Api.Extensions;
using WMS.Modules.Identity.Application.Users;

namespace WMS.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(ISender sender) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginUserCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.ToActionResult();
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return result.ToActionResult();
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var query = new GetCurrentUserQuery(User.GetUserId());
        var result = await sender.Send(query, cancellationToken);

        return result.ToActionResult();
    }
}
