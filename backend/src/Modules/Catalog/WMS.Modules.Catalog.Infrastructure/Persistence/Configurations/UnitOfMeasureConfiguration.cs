using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.Modules.Catalog.Domain;

namespace WMS.Modules.Catalog.Infrastructure.Persistence.Configurations;

internal sealed class UnitOfMeasureConfiguration : IEntityTypeConfiguration<UnitOfMeasure>
{
    public void Configure(EntityTypeBuilder<UnitOfMeasure> builder)
    {
        builder.ToTable("units_of_measure");

        builder.HasKey(unitOfMeasure => unitOfMeasure.Id);
        builder.Property(unitOfMeasure => unitOfMeasure.Id).ValueGeneratedNever();

        builder.Property(unitOfMeasure => unitOfMeasure.Code).HasMaxLength(10).IsRequired();
        builder.HasIndex(unitOfMeasure => unitOfMeasure.Code).IsUnique();

        builder.Property(unitOfMeasure => unitOfMeasure.Name).HasMaxLength(100).IsRequired();
    }
}
