using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using WMS.Modules.Identity.Application.Abstractions;

namespace WMS.Modules.Identity.Infrastructure.Auth;

public sealed class JwtTokenService(IOptions<JwtOptions> options) : ITokenService
{
    /// <summary>
    /// Short role claim type embedded in the token (paired with TokenValidationParameters.RoleClaimType
    /// in Program.cs) instead of the verbose ClaimTypes.Role URI, keeping tokens compact.
    /// </summary>
    public const string RoleClaimType = "role";

    private readonly JwtOptions _options = options.Value;

    public IssuedAccessToken GenerateAccessToken(Guid userId, string email, IReadOnlyCollection<string> roles)
    {
        var expiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(_options.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        claims.AddRange(roles.Select(role => new Claim(RoleClaimType, role)));

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            _options.Issuer,
            _options.Audience,
            claims,
            expires: expiresAtUtc.UtcDateTime,
            signingCredentials: credentials);

        var value = new JwtSecurityTokenHandler().WriteToken(token);

        return new IssuedAccessToken(value, expiresAtUtc);
    }

    public IssuedRefreshToken GenerateRefreshToken()
    {
        var expiresAtUtc = DateTimeOffset.UtcNow.AddDays(_options.RefreshTokenDays);
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var hash = HashRefreshToken(rawToken);

        return new IssuedRefreshToken(rawToken, hash, expiresAtUtc);
    }

    public string HashRefreshToken(string rawRefreshToken)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(rawRefreshToken));

        return Convert.ToHexString(hash);
    }
}
