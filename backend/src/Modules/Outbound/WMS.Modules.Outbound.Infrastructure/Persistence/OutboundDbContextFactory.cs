using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WMS.Modules.Outbound.Infrastructure.Persistence;

/// <summary>
/// Lets `dotnet ef migrations` run against this project directly, independent of WMS.Api's hosting
/// model. Reads the connection string from an env var so no secrets are hardcoded here; falls back
/// to the same local-dev default used in appsettings.json.
/// </summary>
public sealed class OutboundDbContextFactory : IDesignTimeDbContextFactory<OutboundDbContext>
{
    public OutboundDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("WMS_CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=wms;Username=wms_user;Password=wms_password";

        var optionsBuilder = new DbContextOptionsBuilder<OutboundDbContext>();

        optionsBuilder.UseNpgsql(
            connectionString,
            npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", OutboundDbContext.Schema));
        optionsBuilder.UseSnakeCaseNamingConvention();

        return new OutboundDbContext(optionsBuilder.Options);
    }
}
