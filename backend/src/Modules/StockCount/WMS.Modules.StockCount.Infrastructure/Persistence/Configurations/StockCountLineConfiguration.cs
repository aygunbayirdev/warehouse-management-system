using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.Modules.StockCount.Domain;

namespace WMS.Modules.StockCount.Infrastructure.Persistence.Configurations;

internal sealed class StockCountLineConfiguration : IEntityTypeConfiguration<StockCountLine>
{
    public void Configure(EntityTypeBuilder<StockCountLine> builder)
    {
        builder.ToTable("stock_count_lines");

        builder.HasKey(line => line.Id);
        builder.Property(line => line.Id).ValueGeneratedNever();

        builder.Property(line => line.ProductId).IsRequired();
        builder.Property(line => line.SystemQuantity).HasPrecision(18, 4).IsRequired();
        builder.Property(line => line.CountedQuantity).HasPrecision(18, 4).IsRequired();
        builder.Property(line => line.Difference).HasPrecision(18, 4).IsRequired();
    }
}
