using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.Modules.StockCount.Domain;

namespace WMS.Modules.StockCount.Infrastructure.Persistence.Configurations;

internal sealed class StockCountAdjustmentConfiguration : IEntityTypeConfiguration<StockCountAdjustment>
{
    public void Configure(EntityTypeBuilder<StockCountAdjustment> builder)
    {
        builder.ToTable("stock_count_adjustments");

        builder.HasKey(adjustment => adjustment.Id);
        builder.Property(adjustment => adjustment.Id).ValueGeneratedNever();

        builder.Property(adjustment => adjustment.StockCountId).IsRequired();
        builder.Property(adjustment => adjustment.StockCountLineId).IsRequired();
        builder.Property(adjustment => adjustment.WarehouseId).IsRequired();
        builder.Property(adjustment => adjustment.ProductId).IsRequired();
        builder.Property(adjustment => adjustment.DifferenceQuantity).HasPrecision(18, 4).IsRequired();
        builder.Property(adjustment => adjustment.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(adjustment => adjustment.ApprovedByUserId);
        builder.Property(adjustment => adjustment.CreatedAtUtc).IsRequired();
        builder.Property(adjustment => adjustment.DecidedAtUtc);
    }
}
