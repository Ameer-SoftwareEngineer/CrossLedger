using CrossLedger.Domain.Auth;
using CrossLedger.Domain.Fx;
using CrossLedger.Domain.Wallets;
using CrossLedger.Infrastructure.Identity;
using CrossLedger.Infrastructure.Persistence.Configurations;
using CrossLedger.Infrastructure.Persistence.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CrossLedger.Infrastructure.Persistence;

/// <summary>Inherits IdentityDbContext rather than composing a separate identity store,
/// so user/role/token tables and the ledger's own tables share one migration history
/// and one SaveChanges transaction boundary.</summary>
public sealed class CrossLedgerDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    private readonly IDataProtector _totpSecretProtector;

    public CrossLedgerDbContext(DbContextOptions<CrossLedgerDbContext> options, IDataProtectionProvider dataProtectionProvider)
        : base(options)
    {
        _totpSecretProtector = dataProtectionProvider.CreateProtector("CrossLedger.TwoFactorCredential.Secret");
    }

    // Only aggregate roots get a DbSet - LedgerEntry is reached exclusively through
    // Wallet.Entries, matching the repository pattern's aggregate boundary.
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<Quote> Quotes => Set<Quote>();
    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();
    public DbSet<RoutingDecisionRecord> RoutingDecisions => Set<RoutingDecisionRecord>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<TwoFactorCredential> TwoFactorCredentials => Set<TwoFactorCredential>();
    public DbSet<RecoveryCode> RecoveryCodes => Set<RecoveryCode>();
    public DbSet<UsedTotpCode> UsedTotpCodes => Set<UsedTotpCode>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // TwoFactorCredentialConfiguration needs an IDataProtector that only this
        // context (constructed via DI) can supply, so it's excluded from the assembly
        // scan and applied here instead.
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(CrossLedgerDbContext).Assembly,
            type => type != typeof(TwoFactorCredentialConfiguration));
        modelBuilder.ApplyConfiguration(new TwoFactorCredentialConfiguration(_totpSecretProtector));
    }
}
