using Microsoft.EntityFrameworkCore;
using WMS.Modules.Identity.Application.Abstractions;
using WMS.Modules.Identity.Domain;
using WMS.Modules.Identity.Infrastructure.Persistence;

namespace WMS.Modules.Identity.Infrastructure.Repositories;

internal sealed class UserWriteRepository(IdentityDbContext dbContext) : IUserWriteRepository
{
    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken) =>
        Query().FirstOrDefaultAsync(user => user.Email == email, cancellationToken);

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        Query().FirstOrDefaultAsync(user => user.Id == id, cancellationToken);

    public Task<User?> GetByRefreshTokenHashAsync(string tokenHash, CancellationToken cancellationToken) =>
        Query().FirstOrDefaultAsync(user => user.RefreshTokens.Any(rt => rt.TokenHash == tokenHash), cancellationToken);

    public void Add(User user) => dbContext.Users.Add(user);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);

    private IQueryable<User> Query() =>
        dbContext.Users
            .Include(user => user.UserRoles)
            .Include(user => user.RefreshTokens);
}
