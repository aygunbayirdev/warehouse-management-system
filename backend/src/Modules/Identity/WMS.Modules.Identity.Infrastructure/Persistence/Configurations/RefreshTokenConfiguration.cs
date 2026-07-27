using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WMS.Modules.Identity.Domain;

namespace WMS.Modules.Identity.Infrastructure.Persistence.Configurations;

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens");

        builder.HasKey(refreshToken => refreshToken.Id);
        builder.Property(refreshToken => refreshToken.Id).ValueGeneratedNever();

        builder.Property(refreshToken => refreshToken.TokenHash).HasMaxLength(128).IsRequired();
        builder.HasIndex(refreshToken => refreshToken.TokenHash).IsUnique();

        builder.Property(refreshToken => refreshToken.ExpiresAtUtc).IsRequired();
        builder.Property(refreshToken => refreshToken.CreatedAtUtc).IsRequired();
    }
}
