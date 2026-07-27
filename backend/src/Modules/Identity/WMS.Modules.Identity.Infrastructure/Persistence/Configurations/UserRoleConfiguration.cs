using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.Modules.Identity.Domain;

namespace WMS.Modules.Identity.Infrastructure.Persistence.Configurations;

internal sealed class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("user_roles");

        builder.HasKey(userRole => userRole.Id);
        builder.Property(userRole => userRole.Id).ValueGeneratedNever();

        builder.HasIndex(userRole => new { userRole.UserId, userRole.RoleId }).IsUnique();
    }
}
