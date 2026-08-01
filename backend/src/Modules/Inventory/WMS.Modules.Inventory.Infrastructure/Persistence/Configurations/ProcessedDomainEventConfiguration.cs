using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.Modules.Inventory.Domain;

namespace WMS.Modules.Inventory.Infrastructure.Persistence.Configurations;

internal sealed class ProcessedDomainEventConfiguration : IEntityTypeConfiguration<ProcessedDomainEvent>
{
    public void Configure(EntityTypeBuilder<ProcessedDomainEvent> builder)
    {
        builder.ToTable("processed_domain_events");

        builder.HasKey(p => new { p.SourceEventId, p.LineNumber });

        builder.Property(p => p.ProcessedAtUtc).IsRequired();
    }
}
