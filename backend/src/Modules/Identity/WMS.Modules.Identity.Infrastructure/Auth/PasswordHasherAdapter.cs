using Microsoft.AspNetCore.Identity;
using WMS.Modules.Identity.Application.Abstractions;
using WMS.Modules.Identity.Domain;

namespace WMS.Modules.Identity.Infrastructure.Auth;

/// <summary>
/// Wraps ASP.NET Core's battle-tested <see cref="PasswordHasher{TUser}"/> (PBKDF2) so the
/// Application layer depends only on <see cref="IPasswordHasher"/>, not on Identity.Core types.
/// The generic TUser parameter is unused by the default implementation, so passing null is safe.
/// </summary>
public sealed class PasswordHasherAdapter : IPasswordHasher
{
    private readonly PasswordHasher<User> _innerHasher = new();

    public string Hash(string password) => _innerHasher.HashPassword(default!, password);

    public bool Verify(string passwordHash, string providedPassword)
    {
        var result = _innerHasher.VerifyHashedPassword(default!, passwordHash, providedPassword);

        return result is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
    }
}
