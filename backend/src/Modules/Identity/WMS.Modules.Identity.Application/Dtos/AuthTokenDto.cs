namespace WMS.Modules.Identity.Application.Dtos;

public sealed record AuthTokenDto(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAtUtc);
