using CrossLedger.Domain.Fx;
using CrossLedger.Domain.Wallets;
using CrossLedger.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace CrossLedger.Infrastructure.Persistence;

public sealed class CrossLedgerDbContext : DbContext
{
    public CrossLedgerDbContext(DbContextOptions<CrossLedgerDbContext> options) : base(options)
    {
    }

    // Only aggregate roots get a DbSet - LedgerEntry is reached exclusively through
    // Wallet.Entries, matching the repository pattern's aggregate boundary.
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<Quote> Quotes => Set<Quote>();
    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CrossLedgerDbContext).Assembly);
    }
}
