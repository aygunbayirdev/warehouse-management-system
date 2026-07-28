using Microsoft.EntityFrameworkCore;
using WMS.Modules.Transfer.Domain;

namespace WMS.Modules.Transfer.Infrastructure.Persistence;

public sealed class TransferDbContext(DbContextOptions<TransferDbContext> options) : DbContext(options)
{
    public const string Schema = "transfer";

    public DbSet<StockTransfer> StockTransfers => Set<StockTransfer>();

    public DbSet<StockTransferLine> StockTransferLines => Set<StockTransferLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TransferDbContext).Assembly);
    }
}
