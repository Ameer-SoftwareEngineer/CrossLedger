using CrossLedgerWeb.Domain.Auth;
using CrossLedgerWeb.Infrastructure.Persistence.Converters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrossLedgerWeb.Infrastructure.Persistence.Configurations;

public sealed class RecoveryCodeConfiguration : IEntityTypeConfiguration<RecoveryCode>
{
    public void Configure(EntityTypeBuilder<RecoveryCode> builder)
    {
        builder.ToTable("RecoveryCodes");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasConversion(new RecoveryCodeIdConverter()).ValueGeneratedNever();
        builder.Property(c => c.UserId).HasConversion(new UserIdConverter());
        builder.Property(c => c.CodeHash).HasMaxLength(128).IsRequired();
        builder.Property(c => c.CreatedAt);
        builder.Property(c => c.UsedAt);

        builder.HasIndex(c => c.UserId);
    }
}
