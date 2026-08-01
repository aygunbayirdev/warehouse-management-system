using Microsoft.EntityFrameworkCore;
using StockCountAggregate = WMS.Modules.StockCount.Domain.StockCount;
using WMS.BuildingBlocks.Infrastructure.Outbox;
using WMS.Modules.StockCount.Domain;

namespace WMS.Modules.StockCount.Infrastructure.Persistence;

public sealed class StockCountDbContext(DbContextOptions<StockCountDbContext> options) : DbContext(options)
{
    public const string Schema = "stockcount";

    public DbSet<StockCountAggregate> StockCounts => Set<StockCountAggregate>();

    public DbSet<StockCountLine> StockCountLines => Set<StockCountLine>();

    public DbSet<StockCountAdjustment> StockCountAdjustments => Set<StockCountAdjustment>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StockCountDbContext).Assembly);
    }
}
