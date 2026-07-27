using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.Modules.Identity.Domain;

namespace WMS.Modules.Identity.Infrastructure.Persistence.Configurations;

internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");

        builder.HasKey(role => role.Id);
        builder.Property(role => role.Id).ValueGeneratedNever();

        builder.Property(role => role.Name).HasMaxLength(50).IsRequired();
        builder.HasIndex(role => role.Name).IsUnique();

        builder.HasData(
            Role.Create(RoleIds.Admin, RoleNames.Admin),
            Role.Create(RoleIds.WarehouseManager, RoleNames.WarehouseManager),
            Role.Create(RoleIds.WarehouseSupervisor, RoleNames.WarehouseSupervisor),
            Role.Create(RoleIds.WarehouseStaff, RoleNames.WarehouseStaff));
    }
}
