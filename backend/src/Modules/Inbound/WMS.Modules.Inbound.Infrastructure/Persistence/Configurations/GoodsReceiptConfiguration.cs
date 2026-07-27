using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.Modules.Inbound.Domain;

namespace WMS.Modules.Inbound.Infrastructure.Persistence.Configurations;

internal sealed class GoodsReceiptConfiguration : IEntityTypeConfiguration<GoodsReceipt>
{
    public void Configure(EntityTypeBuilder<GoodsReceipt> builder)
    {
        builder.ToTable("goods_receipts");

        builder.HasKey(goodsReceipt => goodsReceipt.Id);
        builder.Property(goodsReceipt => goodsReceipt.Id).ValueGeneratedNever();

        builder.Property(goodsReceipt => goodsReceipt.WarehouseId).IsRequired();
        builder.Property(goodsReceipt => goodsReceipt.CreatedByUserId).IsRequired();
        builder.Property(goodsReceipt => goodsReceipt.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(goodsReceipt => goodsReceipt.CreatedAtUtc).IsRequired();
        builder.Property(goodsReceipt => goodsReceipt.ApprovedAtUtc);

        builder.HasMany(goodsReceipt => goodsReceipt.Lines)
            .WithOne()
            .HasForeignKey(line => line.GoodsReceiptId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(goodsReceipt => goodsReceipt.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
