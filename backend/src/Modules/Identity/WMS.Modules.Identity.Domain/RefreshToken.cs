using WMS.SharedKernel;

namespace WMS.Modules.Identity.Domain;

/// <summary>Owned by the User aggregate. Stores a hash of the refresh token, never the raw value.</summary>
public sealed class RefreshToken : BaseEntity
{
    private RefreshToken()
    {
    }

    private RefreshToken(Guid id, Guid userId, string tokenHash, DateTimeOffset expiresAtUtc)
        : base(id)
    {
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAtUtc = expiresAtUtc;
        CreatedAtUtc = DateTimeOffset.UtcNow;
    }

    public Guid UserId { get; private set; }

    public string TokenHash { get; private set; } = string.Empty;

    public DateTimeOffset ExpiresAtUtc { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; private set; }

    public DateTimeOffset? RevokedAtUtc { get; private set; }

    public bool IsActive => RevokedAtUtc is null && ExpiresAtUtc > DateTimeOffset.UtcNow;

    internal static RefreshToken Issue(Guid userId, string tokenHash, DateTimeOffset expiresAtUtc) =>
        new(Guid.CreateVersion7(), userId, tokenHash, expiresAtUtc);

    internal void Revoke() => RevokedAtUtc = DateTimeOffset.UtcNow;
}
