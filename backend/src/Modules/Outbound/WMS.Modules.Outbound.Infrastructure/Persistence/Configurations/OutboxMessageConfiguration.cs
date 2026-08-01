using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.BuildingBlocks.Infrastructure.Outbox;

namespace WMS.Modules.Outbound.Infrastructure.Persistence.Configurations;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).ValueGeneratedNever();

        builder.Property(m => m.Type).IsRequired().HasMaxLength(500);
        builder.Property(m => m.Payload).IsRequired().HasColumnType("jsonb");
        builder.Property(m => m.OccurredOnUtc).IsRequired();
        builder.Property(m => m.Error).HasMaxLength(2000);

        builder.HasIndex(m => new { m.ProcessedAtUtc, m.OccurredOnUtc });
    }
}
