using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.Modules.Outbound.Domain;

namespace WMS.Modules.Outbound.Infrastructure.Persistence.Configurations;

internal sealed class GoodsIssueConfiguration : IEntityTypeConfiguration<GoodsIssue>
{
    public void Configure(EntityTypeBuilder<GoodsIssue> builder)
    {
        builder.ToTable("goods_issues");

        builder.HasKey(goodsIssue => goodsIssue.Id);
        builder.Property(goodsIssue => goodsIssue.Id).ValueGeneratedNever();

        builder.Property(goodsIssue => goodsIssue.WarehouseId).IsRequired();
        builder.Property(goodsIssue => goodsIssue.Destination).HasMaxLength(200).IsRequired();
        builder.Property(goodsIssue => goodsIssue.CreatedByUserId).IsRequired();
        builder.Property(goodsIssue => goodsIssue.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(goodsIssue => goodsIssue.CreatedAtUtc).IsRequired();
        builder.Property(goodsIssue => goodsIssue.ApprovedAtUtc);

        builder.HasMany(goodsIssue => goodsIssue.Lines)
            .WithOne()
            .HasForeignKey(line => line.GoodsIssueId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(goodsIssue => goodsIssue.Lines).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
