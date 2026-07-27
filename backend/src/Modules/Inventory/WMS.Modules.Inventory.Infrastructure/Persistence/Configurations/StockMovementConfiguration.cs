using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.Modules.Inventory.Domain;

namespace WMS.Modules.Inventory.Infrastructure.Persistence.Configurations;

internal sealed class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("stock_movements");

        builder.HasKey(stockMovement => stockMovement.Id);
        builder.Property(stockMovement => stockMovement.Id).ValueGeneratedNever();

        builder.Property(stockMovement => stockMovement.WarehouseId).IsRequired();
        builder.Property(stockMovement => stockMovement.ProductId).IsRequired();
        builder.Property(stockMovement => stockMovement.Type).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(stockMovement => stockMovement.Quantity).HasPrecision(18, 4).IsRequired();
        builder.Property(stockMovement => stockMovement.Reason).HasMaxLength(200).IsRequired();
        builder.Property(stockMovement => stockMovement.OccurredAtUtc).IsRequired();

        builder.HasIndex(stockMovement => new { stockMovement.WarehouseId, stockMovement.ProductId });
    }
}
