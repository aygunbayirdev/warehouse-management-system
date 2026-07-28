using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.Modules.Outbound.Domain;

namespace WMS.Modules.Outbound.Infrastructure.Persistence.Configurations;

internal sealed class GoodsIssueLineConfiguration : IEntityTypeConfiguration<GoodsIssueLine>
{
    public void Configure(EntityTypeBuilder<GoodsIssueLine> builder)
    {
        builder.ToTable("goods_issue_lines");

        builder.HasKey(line => line.Id);
        builder.Property(line => line.Id).ValueGeneratedNever();

        builder.Property(line => line.ProductId).IsRequired();
        builder.Property(line => line.Quantity).HasPrecision(18, 4).IsRequired();
    }
}
