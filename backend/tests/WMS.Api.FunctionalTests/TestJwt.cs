using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WMS.Modules.Identity.Infrastructure.Auth;

namespace WMS.Api.FunctionalTests;

/// <summary>
/// Mints a JWT with the same shape <see cref="JwtTokenService"/> issues, signed with the app's real
/// (test-only) secret pulled from the running factory's DI container. Used to test role-based
/// authorization for roles other than the seeded Admin, without needing a user-management endpoint
/// to create additional accounts.
/// </summary>
public static class TestJwt
{
    public static string CreateForRole(CustomWebApplicationFactory factory, string role)
    {
        using var scope = factory.Services.CreateScope();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<JwtOptions>>().Value;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Email, "staff@wms.local"),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtTokenService.RoleClaimType, role),
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Secret));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            options.Issuer,
            options.Audience,
            claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
