using WMS.Modules.Identity.Domain;

namespace WMS.Modules.Identity.Application.Abstractions;

public interface IUserWriteRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);

    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<User?> GetByRefreshTokenHashAsync(string tokenHash, CancellationToken cancellationToken);

    void Add(User user);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
