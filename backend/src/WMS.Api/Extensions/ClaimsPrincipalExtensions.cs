using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace WMS.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var subject = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (subject is null || !Guid.TryParse(subject, out var userId))
        {
            throw new InvalidOperationException("The current principal does not contain a valid user id claim.");
        }

        return userId;
    }
}
