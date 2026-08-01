using Microsoft.EntityFrameworkCore;
using WMS.BuildingBlocks.Infrastructure.Outbox;
using WMS.Modules.Inbound.Domain;

namespace WMS.Modules.Inbound.Infrastructure.Persistence;

public sealed class InboundDbContext(DbContextOptions<InboundDbContext> options) : DbContext(options)
{
    public const string Schema = "inbound";

    public DbSet<GoodsReceipt> GoodsReceipts => Set<GoodsReceipt>();

    public DbSet<GoodsReceiptLine> GoodsReceiptLines => Set<GoodsReceiptLine>();

    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InboundDbContext).Assembly);
    }
}
