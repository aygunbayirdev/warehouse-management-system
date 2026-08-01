using Microsoft.EntityFrameworkCore;
using WMS.BuildingBlocks.Infrastructure.Outbox;
using WMS.Modules.Outbound.Domain;

namespace WMS.Modules.Outbound.Infrastructure.Persistence;

public sealed class OutboundDbContext(DbContextOptions<OutboundDbContext> options) : DbContext(options)
{
    public const string Schema = "outbound";

    public DbSet<GoodsIssue> GoodsIssues => Set<GoodsIssue>();

    public DbSet<GoodsIssueLine> GoodsIssueLines => Set<GoodsIssueLine>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OutboundDbContext).Assembly);
    }
}
