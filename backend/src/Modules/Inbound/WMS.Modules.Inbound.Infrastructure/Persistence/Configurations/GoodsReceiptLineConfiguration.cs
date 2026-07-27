using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.Modules.Inbound.Domain;

namespace WMS.Modules.Inbound.Infrastructure.Persistence.Configurations;

internal sealed class GoodsReceiptLineConfiguration : IEntityTypeConfiguration<GoodsReceiptLine>
{
    public void Configure(EntityTypeBuilder<GoodsReceiptLine> builder)
    {
        builder.ToTable("goods_receipt_lines");

        builder.HasKey(line => line.Id);
        builder.Property(line => line.Id).ValueGeneratedNever();

        builder.Property(line => line.ProductId).IsRequired();
        builder.Property(line => line.Quantity).HasPrecision(18, 4).IsRequired();
    }
}
