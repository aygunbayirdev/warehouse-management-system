using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StockCountAggregate = WMS.Modules.StockCount.Domain.StockCount;
using WMS.Modules.StockCount.Domain;

namespace WMS.Modules.StockCount.Infrastructure.Persistence.Configurations;

internal sealed class StockCountConfiguration : IEntityTypeConfiguration<StockCountAggregate>
{
    public void Configure(EntityTypeBuilder<StockCountAggregate> builder)
    {
        builder.ToTable("stock_counts");

        builder.HasKey(stockCount => stockCount.Id);
        builder.Property(stockCount => stockCount.Id).ValueGeneratedNever();

        builder.Property(stockCount => stockCount.WarehouseId).IsRequired();
        builder.Property(stockCount => stockCount.CreatedByUserId).IsRequired();
        builder.Property(stockCount => stockCount.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(stockCount => stockCount.CreatedAtUtc).IsRequired();
        builder.Property(stockCount => stockCount.ClosedAtUtc);

        builder.HasMany(stockCount => stockCount.Lines)
            .WithOne()
            .HasForeignKey(line => line.StockCountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(stockCount => stockCount.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
