using FluentAssertions;
using NSubstitute;
using WMS.Modules.Identity.Application.Abstractions;
using WMS.Modules.Identity.Application.Users;
using WMS.Modules.Identity.Domain;

namespace WMS.Modules.Identity.UnitTests;

public class LoginUserCommandHandlerTests
{
    private readonly IUserWriteRepository _userWriteRepository = Substitute.For<IUserWriteRepository>();
    private readonly IPasswordHasher _passwordHasher = Substitute.For<IPasswordHasher>();
    private readonly ITokenService _tokenService = Substitute.For<ITokenService>();
    private readonly LoginUserCommandHandler _handler;

    public LoginUserCommandHandlerTests()
    {
        _handler = new LoginUserCommandHandler(_userWriteRepository, _passwordHasher, _tokenService);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ReturnsTokensAndIssuesRefreshToken()
    {
        var user = User.Create("admin@wms.local", "hashed-password", "System", "Admin");
        user.AssignRole(RoleIds.Admin);

        _userWriteRepository.GetByEmailAsync("admin@wms.local", Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.Verify("hashed-password", "ChangeMe123!").Returns(true);
        _tokenService.GenerateAccessToken(user.Id, user.Email, Arg.Is<IReadOnlyCollection<string>>(roles => roles.Contains(RoleNames.Admin)))
            .Returns(new IssuedAccessToken("access-token", DateTimeOffset.UtcNow.AddMinutes(30)));
        _tokenService.GenerateRefreshToken()
            .Returns(new IssuedRefreshToken("refresh-token", "refresh-hash", DateTimeOffset.UtcNow.AddDays(7)));

        var result = await _handler.Handle(new LoginUserCommand("admin@wms.local", "ChangeMe123!"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("access-token");
        result.Value.RefreshToken.Should().Be("refresh-token");
        user.RefreshTokens.Should().ContainSingle(refreshToken => refreshToken.TokenHash == "refresh-hash");
        await _userWriteRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithUnknownEmail_ReturnsInvalidCredentials()
    {
        _userWriteRepository.GetByEmailAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var result = await _handler.Handle(new LoginUserCommand("nobody@wms.local", "whatever"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.InvalidCredentials");
    }

    [Fact]
    public async Task Handle_WithWrongPassword_ReturnsInvalidCredentials()
    {
        var user = User.Create("admin@wms.local", "hashed-password", "System", "Admin");

        _userWriteRepository.GetByEmailAsync("admin@wms.local", Arg.Any<CancellationToken>())
            .Returns(user);
        _passwordHasher.Verify("hashed-password", "wrong-password").Returns(false);

        var result = await _handler.Handle(new LoginUserCommand("admin@wms.local", "wrong-password"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.InvalidCredentials");
    }

    [Fact]
    public async Task Handle_WithDeactivatedUser_ReturnsInvalidCredentials()
    {
        var user = User.Create("admin@wms.local", "hashed-password", "System", "Admin");
        user.Deactivate();

        _userWriteRepository.GetByEmailAsync("admin@wms.local", Arg.Any<CancellationToken>())
            .Returns(user);

        var result = await _handler.Handle(new LoginUserCommand("admin@wms.local", "ChangeMe123!"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Auth.InvalidCredentials");
    }
}
