using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WMS.Modules.Identity.Application.Abstractions;
using WMS.Modules.Identity.Domain;
using WMS.Modules.Identity.Infrastructure.Persistence;

namespace WMS.Modules.Identity.Infrastructure.Seeding;

/// <summary>
/// Applies pending migrations and ensures a default Admin user exists. Called once from
/// WMS.Api's startup; idempotent, so it is safe to run on every app start in development.
/// </summary>
public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider rootServices, CancellationToken cancellationToken = default)
    {
        using var scope = rootServices.CreateScope();
        var services = scope.ServiceProvider;

        var dbContext = services.GetRequiredService<IdentityDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);

        var adminOptions = services.GetRequiredService<IOptions<AdminSeedOptions>>().Value;

        var adminExists = await dbContext.Users.AnyAsync(user => user.Email == adminOptions.Email, cancellationToken);

        if (adminExists)
        {
            return;
        }

        var passwordHasher = services.GetRequiredService<IPasswordHasher>();

        var admin = User.Create(
            adminOptions.Email,
            passwordHasher.Hash(adminOptions.Password),
            adminOptions.FirstName,
            adminOptions.LastName);

        admin.AssignRole(RoleIds.Admin);

        dbContext.Users.Add(admin);
        await dbContext.SaveChangesAsync(cancellationToken);

        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("WMS.Modules.Identity.Seeding");

        logger.LogWarning(
            "Seeded default admin user {Email}. Change the password immediately outside local development.",
            adminOptions.Email);
    }
}
