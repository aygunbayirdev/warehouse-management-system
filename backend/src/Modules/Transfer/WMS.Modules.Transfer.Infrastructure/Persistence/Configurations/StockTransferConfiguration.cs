using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.Modules.Transfer.Domain;

namespace WMS.Modules.Transfer.Infrastructure.Persistence.Configurations;

internal sealed class StockTransferConfiguration : IEntityTypeConfiguration<StockTransfer>
{
    public void Configure(EntityTypeBuilder<StockTransfer> builder)
    {
        builder.ToTable("stock_transfers");

        builder.HasKey(stockTransfer => stockTransfer.Id);
        builder.Property(stockTransfer => stockTransfer.Id).ValueGeneratedNever();

        builder.Property(stockTransfer => stockTransfer.SourceWarehouseId).IsRequired();
        builder.Property(stockTransfer => stockTransfer.DestinationWarehouseId).IsRequired();
        builder.Property(stockTransfer => stockTransfer.CreatedByUserId).IsRequired();
        builder.Property(stockTransfer => stockTransfer.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(stockTransfer => stockTransfer.CreatedAtUtc).IsRequired();
        builder.Property(stockTransfer => stockTransfer.ShippedAtUtc);
        builder.Property(stockTransfer => stockTransfer.ReceivedAtUtc);

        builder.HasMany(stockTransfer => stockTransfer.Lines)
            .WithOne()
            .HasForeignKey(line => line.StockTransferId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(stockTransfer => stockTransfer.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
