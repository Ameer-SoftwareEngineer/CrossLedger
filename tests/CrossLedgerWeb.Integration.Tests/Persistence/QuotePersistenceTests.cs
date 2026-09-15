using CrossLedgerWeb.Domain.Fx;
using CrossLedgerWeb.Domain.ValueObjects;
using CrossLedgerWeb.Infrastructure.Persistence;
using CrossLedgerWeb.Infrastructure.Repositories;
using CrossLedgerWeb.Integration.Tests.TestSupport;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CrossLedgerWeb.Integration.Tests.Persistence;

public sealed class QuotePersistenceTests : IDisposable
{
    private static readonly Currency Usd = Currency.From("USD");
    private static readonly Currency Pkr = Currency.From("PKR");

    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<CrossLedgerWebDbContext> _options;

    public QuotePersistenceTests()
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
    public async Task A_quote_round_trips_with_its_computed_customer_rate_and_expiry_preserved()
    {
        var now = DateTimeOffset.UtcNow;
        var quote = new Quote(QuoteId.New(), Usd, Pkr, midMarketRate: 279.90m, spreadRate: 0.005m, now, TimeSpan.FromSeconds(30));

        await using (var writeContext = new CrossLedgerWebDbContext(_options, TestDataProtection.Provider))
        {
            writeContext.Quotes.Add(quote);
            await writeContext.SaveChangesAsync();
        }

        await using var readContext = new CrossLedgerWebDbContext(_options, TestDataProtection.Provider);
        var reloaded = await new QuoteRepository(readContext).GetByIdAsync(quote.Id, CancellationToken.None);

        reloaded.Should().NotBeNull();
        reloaded!.CustomerRate.Should().Be(quote.CustomerRate);
        reloaded.ExpiresAt.Should().Be(quote.ExpiresAt);
        reloaded.FromCurrency.Should().Be(Usd);
        reloaded.ToCurrency.Should().Be(Pkr);

        // The rehydrated quote must behave identically to a freshly-issued one.
        reloaded.Convert(new Money(100m, Usd), now).Should().Be(quote.Convert(new Money(100m, Usd), now));
    }
}
