using WMS.BuildingBlocks.Application.Messaging;
using WMS.Modules.Identity.Application.Abstractions;
using WMS.Modules.Identity.Application.Dtos;
using WMS.Modules.Identity.Domain;
using WMS.SharedKernel;

namespace WMS.Modules.Identity.Application.Users;

public sealed class LoginUserCommandHandler(
    IUserWriteRepository userWriteRepository,
    IPasswordHasher passwordHasher,
    ITokenService tokenService)
    : ICommandHandler<LoginUserCommand, AuthTokenDto>
{
    public async Task<Result<AuthTokenDto>> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userWriteRepository.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), cancellationToken);

        if (user is null || !user.IsActive || !passwordHasher.Verify(user.PasswordHash, request.Password))
        {
            return Result.Failure<AuthTokenDto>(
                Error.Unauthorized("Auth.InvalidCredentials", "Email or password is incorrect."));
        }

        var roleNames = user.UserRoles
            .Select(userRole => RoleCatalog.NamesById.GetValueOrDefault(userRole.RoleId))
            .Where(name => name is not null)
            .Select(name => name!)
            .ToList();

        var accessToken = tokenService.GenerateAccessToken(user.Id, user.Email, roleNames);
        var refreshToken = tokenService.GenerateRefreshToken();

        user.IssueRefreshToken(refreshToken.Hash, refreshToken.ExpiresAtUtc);

        await userWriteRepository.SaveChangesAsync(cancellationToken);

        return new AuthTokenDto(
            accessToken.Value,
            accessToken.ExpiresAtUtc,
            refreshToken.Value,
            refreshToken.ExpiresAtUtc);
    }
}
