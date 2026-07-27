using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Identity.Application.Abstractions;
using WMS.Modules.Identity.Application.Dtos;
using WMS.Modules.Identity.Domain;
using WMS.SharedKernel;

namespace WMS.Modules.Identity.Application.Users;

public sealed class RefreshTokenCommandHandler(
    IUserWriteRepository userWriteRepository,
    ITokenService tokenService)
    : ICommandHandler<RefreshTokenCommand, AuthTokenDto>
{
    public async Task<Result<AuthTokenDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = tokenService.HashRefreshToken(request.RefreshToken);

        var user = await userWriteRepository.GetByRefreshTokenHashAsync(tokenHash, cancellationToken);

        if (user is null || !user.IsActive)
        {
            return Result.Failure<AuthTokenDto>(
                Error.Unauthorized("Auth.InvalidRefreshToken", "The refresh token is invalid or expired."));
        }

        var revokeResult = user.RevokeRefreshToken(tokenHash);

        if (revokeResult.IsFailure)
        {
            return Result.Failure<AuthTokenDto>(revokeResult.Error);
        }

        var roleNames = user.UserRoles
            .Select(userRole => RoleCatalog.NamesById.GetValueOrDefault(userRole.RoleId))
            .Where(name => name is not null)
            .Select(name => name!)
            .ToList();

        var accessToken = tokenService.GenerateAccessToken(user.Id, user.Email, roleNames);
        var newRefreshToken = tokenService.GenerateRefreshToken();

        user.IssueRefreshToken(newRefreshToken.Hash, newRefreshToken.ExpiresAtUtc);

        await userWriteRepository.SaveChangesAsync(cancellationToken);

        return new AuthTokenDto(
            accessToken.Value,
            accessToken.ExpiresAtUtc,
            newRefreshToken.Value,
            newRefreshToken.ExpiresAtUtc);
    }
}
