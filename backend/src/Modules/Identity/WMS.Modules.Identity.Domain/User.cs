using WMS.SharedKernel;

namespace WMS.Modules.Identity.Domain;

public sealed class User : BaseEntity
{
    private readonly List<UserRole> _userRoles = [];
    private readonly List<RefreshToken> _refreshTokens = [];

    private User()
    {
    }

    private User(Guid id, string email, string passwordHash, string firstName, string lastName)
        : base(id)
    {
        Email = email;
        PasswordHash = passwordHash;
        FirstName = firstName;
        LastName = lastName;
        IsActive = true;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public string Email { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public string FirstName { get; private set; } = string.Empty;

    public string LastName { get; private set; } = string.Empty;

    public bool IsActive { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();

    public static User Create(string email, string passwordHash, string firstName, string lastName)
    {
        Guard.AgainstNullOrWhiteSpace(email, nameof(email));
        Guard.AgainstNullOrWhiteSpace(passwordHash, nameof(passwordHash));
        Guard.AgainstNullOrWhiteSpace(firstName, nameof(firstName));
        Guard.AgainstNullOrWhiteSpace(lastName, nameof(lastName));

        return new User(Guid.CreateVersion7(), email.Trim().ToLowerInvariant(), passwordHash, firstName, lastName);
    }

    public void AssignRole(Guid roleId)
    {
        if (_userRoles.Any(userRole => userRole.RoleId == roleId))
        {
            return;
        }

        _userRoles.Add(UserRole.Create(Id, roleId));
    }

    public RefreshToken IssueRefreshToken(string tokenHash, DateTimeOffset expiresAtUtc)
    {
        Guard.AgainstNullOrWhiteSpace(tokenHash, nameof(tokenHash));

        var refreshToken = RefreshToken.Issue(Id, tokenHash, expiresAtUtc);
        _refreshTokens.Add(refreshToken);

        return refreshToken;
    }

    public Result RevokeRefreshToken(string tokenHash)
    {
        var refreshToken = _refreshTokens.FirstOrDefault(rt => rt.TokenHash == tokenHash);

        if (refreshToken is null || !refreshToken.IsActive)
        {
            return Result.Failure(
                Error.NotFound("RefreshToken.NotFound", "The refresh token was not found or is no longer active."));
        }

        refreshToken.Revoke();

        return Result.Success();
    }

    public void Deactivate() => IsActive = false;
}
