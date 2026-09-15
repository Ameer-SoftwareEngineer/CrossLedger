using CrossLedgerWeb.Domain.ValueObjects;
using CrossLedgerWeb.Domain.Wallets;
using CrossLedgerWeb.Infrastructure.Persistence;
using CrossLedgerWeb.Infrastructure.Repositories;
using CrossLedgerWeb.Integration.Tests.TestSupport;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CrossLedgerWeb.Integration.Tests.Persistence;

// SQLite in-memory stands in for the Testcontainers + real SQL Server setup the
// specification calls for (section 3.6) - this environment has no Docker available.
// Unlike the EF Core InMemory provider (which has known gaps materializing complex
// types - see dotnet/efcore#31464), SQLite goes through the same relational query
// pipeline SQL Server does, so this actually exercises the constructor-binding,
// backing-field navigation, and complex-type mappings in Configurations/*.
public sealed class WalletPersistenceTests : IDisposable
{
    private static readonly Currency Usd = Currency.From("USD");

    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<CrossLedgerWebDbContext> _options;

    public WalletPersistenceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<CrossLedgerWebDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new CrossLedgerWebDbContext(_options, TestDataProtection.Provider);
        context.Database.EnsureCreated();
    }

    public void Dispose() => _connection.Dispose();

    [Fact]
    public async Task A_wallet_and_its_ledger_entries_round_trip_through_ef_core()
    {
        var walletId = WalletId.New();
        var now = DateTimeOffset.UtcNow;

        await using (var writeContext = new CrossLedgerWebDbContext(_options, TestDataProtection.Provider))
        {
            var wallet = new Wallet(walletId, UserId.New(), Usd);
            wallet.Credit(new Money(100m, Usd), TransferId.New(), now);
            wallet.Debit(new Money(40m, Usd), TransferId.New(), now);

            writeContext.Wallets.Add(wallet);
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = new CrossLedgerWebDbContext(_options, TestDataProtection.Provider);
        var reloaded = await new WalletRepository(readContext).GetByIdAsync(walletId, CancellationToken.None);

        reloaded.Should().NotBeNull();
        reloaded!.Balance.Should().Be(new Money(60m, Usd));
        reloaded.Entries.Should().HaveCount(2);
        reloaded.Kind.Should().Be(WalletKind.Customer);
    }

    [Fact]
    public async Task A_system_clearing_wallets_negative_balance_round_trips_correctly()
    {
        var walletId = WalletId.New();
        var now = DateTimeOffset.UtcNow;

        await using (var writeContext = new CrossLedgerWebDbContext(_options, TestDataProtection.Provider))
        {
            var wallet = new Wallet(walletId, UserId.New(), Usd, WalletKind.SystemClearing);
            wallet.Debit(new Money(500m, Usd), TransferId.New(), now);

            writeContext.Wallets.Add(wallet);
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = new CrossLedgerWebDbContext(_options, TestDataProtection.Provider);
        var reloaded = await new WalletRepository(readContext).GetByIdAsync(walletId, CancellationToken.None);

        reloaded!.Balance.Amount.Should().Be(-500m);
    }

    [Fact]
    public async Task Reloaded_ledger_entries_preserve_direction_and_signed_amount()
    {
        var walletId = WalletId.New();
        var now = DateTimeOffset.UtcNow;

        await using (var writeContext = new CrossLedgerWebDbContext(_options, TestDataProtection.Provider))
        {
            var wallet = new Wallet(walletId, UserId.New(), Usd);
            wallet.Credit(new Money(100m, Usd), TransferId.New(), now);

            writeContext.Wallets.Add(wallet);
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = new CrossLedgerWebDbContext(_options, TestDataProtection.Provider);
        var reloaded = await new WalletRepository(readContext).GetByIdAsync(walletId, CancellationToken.None);

        var entry = reloaded!.Entries.Single();
        entry.Amount.Should().Be(new Money(100m, Usd));
        entry.SignedAmount.Should().Be(new Money(100m, Usd));
    }
}
