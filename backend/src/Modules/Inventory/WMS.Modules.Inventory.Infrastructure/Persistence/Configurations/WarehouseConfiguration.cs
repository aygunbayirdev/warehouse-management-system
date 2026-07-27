using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.Modules.Inventory.Domain;

namespace WMS.Modules.Inventory.Infrastructure.Persistence.Configurations;

internal sealed class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("warehouses");

        builder.HasKey(warehouse => warehouse.Id);
        builder.Property(warehouse => warehouse.Id).ValueGeneratedNever();

        builder.Property(warehouse => warehouse.Code).HasMaxLength(20).IsRequired();
        builder.HasIndex(warehouse => warehouse.Code).IsUnique();

        builder.Property(warehouse => warehouse.Name).HasMaxLength(150).IsRequired();
        builder.Property(warehouse => warehouse.Address).HasMaxLength(300);
    }
}
