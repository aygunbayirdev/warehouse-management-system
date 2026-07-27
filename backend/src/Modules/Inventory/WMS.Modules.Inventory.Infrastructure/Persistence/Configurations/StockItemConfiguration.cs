using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.Modules.Inventory.Domain;

namespace WMS.Modules.Inventory.Infrastructure.Persistence.Configurations;

internal sealed class StockItemConfiguration : IEntityTypeConfiguration<StockItem>
{
    public void Configure(EntityTypeBuilder<StockItem> builder)
    {
        builder.ToTable("stock_items");

        builder.HasKey(stockItem => stockItem.Id);
        builder.Property(stockItem => stockItem.Id).ValueGeneratedNever();

        builder.Property(stockItem => stockItem.WarehouseId).IsRequired();
        builder.Property(stockItem => stockItem.ProductId).IsRequired();
        builder.Property(stockItem => stockItem.Quantity).HasPrecision(18, 4).IsRequired();

        builder.HasIndex(stockItem => new { stockItem.WarehouseId, stockItem.ProductId }).IsUnique();

        // Concurrent writers (Inbound/Outbound/Transfer/StockCount approvals) all update Quantity on
        // this same row; Postgres' xmin system column is used as the optimistic concurrency token
        // (see CLAUDE.md "EF Core Kuralları").
        builder.Property<uint>("xmin").IsRowVersion();
    }
}
