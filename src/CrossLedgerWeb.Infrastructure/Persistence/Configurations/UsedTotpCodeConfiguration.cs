using CrossLedgerWeb.Domain.Auth;
using CrossLedgerWeb.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrossLedgerWeb.Infrastructure.Persistence.Configurations;

public sealed class UsedTotpCodeConfiguration : IEntityTypeConfiguration<UsedTotpCode>
{
    public void Configure(EntityTypeBuilder<UsedTotpCode> builder)
    {
        builder.ToTable("UsedTotpCodes");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasConversion(new UsedTotpCodeIdConverter()).ValueGeneratedNever();
        builder.Property(c => c.UserId).HasConversion(new UserIdConverter());
        builder.Property(c => c.Code).HasMaxLength(6).IsRequired();
        builder.Property(c => c.UsedAt);
        builder.Property(c => c.ExpiresAt);

        // The replay check looks up by exactly this pair - a covering index keeps it
        // an index-only seek instead of a scan on a table that's high write volume.
        builder.HasIndex(c => new { c.UserId, c.Code });
    }
}
