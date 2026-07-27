namespace WMS.Modules.Identity.Application.Abstractions;

public sealed record IssuedAccessToken(string Value, DateTimeOffset ExpiresAtUtc);

public sealed record IssuedRefreshToken(string Value, string Hash, DateTimeOffset ExpiresAtUtc);

public interface ITokenService
{
    IssuedAccessToken GenerateAccessToken(Guid userId, string email, IReadOnlyCollection<string> roles);

    IssuedRefreshToken GenerateRefreshToken();

    string HashRefreshToken(string rawRefreshToken);
}
