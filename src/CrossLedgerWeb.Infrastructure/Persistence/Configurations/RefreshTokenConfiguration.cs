using CrossLedgerWeb.Domain.Auth;
using CrossLedgerWeb.Domain.ValueObjects;
using CrossLedgerWeb.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrossLedgerWeb.Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).HasConversion(new RefreshTokenIdConverter()).ValueGeneratedNever();
        builder.Property(t => t.UserId).HasConversion(new UserIdConverter());
        builder.Property(t => t.TokenHash).HasMaxLength(128).IsRequired();
        builder.Property(t => t.IssuedAt);
        builder.Property(t => t.ExpiresAt);
        builder.Property(t => t.RevokedAt);

        builder.Property(t => t.ReplacedByTokenId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? new RefreshTokenId(value.Value) : (RefreshTokenId?)null);

        // A unique index on the hash, not just an ordinary one: the hash IS the lookup
        // key when a client presents a refresh token, and two different tokens hashing
        // to the same value would mean one masked a genuine collision.
        builder.HasIndex(t => t.TokenHash).IsUnique();
        builder.HasIndex(t => t.FamilyId);
        builder.HasIndex(t => t.UserId);
    }
}
