using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.Modules.Catalog.Domain;

namespace WMS.Modules.Catalog.Infrastructure.Persistence.Configurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");

        builder.HasKey(product => product.Id);
        builder.Property(product => product.Id).ValueGeneratedNever();

        builder.Property(product => product.Sku).HasMaxLength(50).IsRequired();
        builder.HasIndex(product => product.Sku).IsUnique();

        builder.Property(product => product.Name).HasMaxLength(200).IsRequired();
        builder.Property(product => product.MinStockQuantity).HasPrecision(18, 4).IsRequired();

        // Restrict, not Cascade: deleting a UnitOfMeasure/Category while a Product still
        // references it must fail loudly rather than silently orphaning stock data later.
        builder.HasOne<UnitOfMeasure>()
            .WithMany()
            .HasForeignKey(product => product.UnitOfMeasureId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(product => product.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
