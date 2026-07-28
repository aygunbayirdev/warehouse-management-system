using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace WMS.Modules.StockCount.Infrastructure.Persistence;

/// <summary>
/// Lets `dotnet ef migrations` run against this project directly, independent of WMS.Api's hosting
/// model. Reads the connection string from an env var so no secrets are hardcoded here; falls back
/// to the same local-dev default used in appsettings.json.
/// </summary>
public sealed class StockCountDbContextFactory : IDesignTimeDbContextFactory<StockCountDbContext>
{
    public StockCountDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("WMS_CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=wms;Username=wms_user;Password=wms_password";

        var optionsBuilder = new DbContextOptionsBuilder<StockCountDbContext>();

        optionsBuilder.UseNpgsql(
            connectionString,
            npgsql => npgsql.MigrationsHistoryTable("__ef_migrations_history", StockCountDbContext.Schema));
        optionsBuilder.UseSnakeCaseNamingConvention();

        return new StockCountDbContext(optionsBuilder.Options);
    }
}
