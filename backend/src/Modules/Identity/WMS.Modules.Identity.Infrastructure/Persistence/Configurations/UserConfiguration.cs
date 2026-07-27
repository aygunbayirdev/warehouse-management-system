using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.Modules.Identity.Domain;

namespace WMS.Modules.Identity.Infrastructure.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(user => user.Id);
        builder.Property(user => user.Id).ValueGeneratedNever();

        builder.Property(user => user.Email).HasMaxLength(256).IsRequired();
        builder.HasIndex(user => user.Email).IsUnique();

        builder.Property(user => user.PasswordHash).IsRequired();
        builder.Property(user => user.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(user => user.LastName).HasMaxLength(100).IsRequired();
        builder.Property(user => user.IsActive).IsRequired();
        builder.Property(user => user.CreatedAtUtc).IsRequired();

        builder.HasMany(user => user.UserRoles)
            .WithOne()
            .HasForeignKey(userRole => userRole.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(user => user.RefreshTokens)
            .WithOne()
            .HasForeignKey(refreshToken => refreshToken.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(user => user.UserRoles).UsePropertyAccessMode(PropertyAccessMode.Field);
        builder.Navigation(user => user.RefreshTokens).UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
