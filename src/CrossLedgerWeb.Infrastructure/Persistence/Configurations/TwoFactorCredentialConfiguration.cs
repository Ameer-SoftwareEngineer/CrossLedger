using CrossLedgerWeb.Domain.Auth;
using CrossLedgerWeb.Infrastructure.Persistence.Converters;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CrossLedgerWeb.Infrastructure.Persistence.Configurations;

/// <summary>
/// Not picked up by ApplyConfigurationsFromAssembly - it needs an IDataProtector, which
/// only the DbContext (resolved via DI) can supply, so CrossLedgerWebDbContext excludes
/// this type from the assembly scan and applies it manually with a protector passed in.
/// Secrets encrypted at rest (specification 6.4): the Data Protection API's key ring is
/// file-system-based locally and would be configured to persist keys to Azure Key Vault
/// in production (PersistKeysToAzureKeyVault) - that wiring is an infra/deployment
/// concern, not something this configuration class needs to know about.
/// </summary>
public sealed class TwoFactorCredentialConfiguration : IEntityTypeConfiguration<TwoFactorCredential>
{
    private readonly IDataProtector _secretProtector;

    public TwoFactorCredentialConfiguration(IDataProtector secretProtector)
    {
        _secretProtector = secretProtector;
    }

    public void Configure(EntityTypeBuilder<TwoFactorCredential> builder)
    {
        builder.ToTable("TwoFactorCredentials");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasConversion(new TwoFactorCredentialIdConverter()).ValueGeneratedNever();
        builder.Property(c => c.UserId).HasConversion(new UserIdConverter());
        builder.Property(c => c.EnrolledAt);
        builder.Property(c => c.IsEnabled);

        builder.Property(c => c.Secret)
            .HasConversion(plain => _secretProtector.Protect(plain), encrypted => _secretProtector.Unprotect(encrypted))
            .HasMaxLength(512); // protected payloads run longer than the raw Base32 secret

        builder.HasIndex(c => c.UserId).IsUnique();
    }
}
