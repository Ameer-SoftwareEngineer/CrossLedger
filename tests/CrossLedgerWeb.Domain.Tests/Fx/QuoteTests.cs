using CrossLedgerWeb.Domain.Exceptions;
using CrossLedgerWeb.Domain.Fx;
using CrossLedgerWeb.Domain.ValueObjects;
using FluentAssertions;

namespace CrossLedgerWeb.Domain.Tests.Fx;

public class QuoteTests
{
    private static readonly Currency Usd = Currency.From("USD");
    private static readonly Currency Pkr = Currency.From("PKR");
    private static readonly DateTimeOffset IssuedAt = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    private static Quote NewQuote(decimal midMarketRate = 279.90m, decimal spreadRate = 0.005m, TimeSpan? validFor = null) =>
        new(QuoteId.New(), Usd, Pkr, midMarketRate, spreadRate, IssuedAt, validFor ?? TimeSpan.FromSeconds(30));

    [Fact]
    public void CustomerRate_applies_the_spread_against_the_mid_market_rate()
    {
        // specification section 2.2: mid-market 279.90, spread 0.50% -> customer rate ~278.50
        var quote = NewQuote(midMarketRate: 279.90m, spreadRate: 0.005m);

        quote.CustomerRate.Should().Be(278.5005m);
    }

    [Fact]
    public void ExpiresAt_is_issuedAt_plus_the_validity_window()
    {
        var quote = NewQuote(validFor: TimeSpan.FromSeconds(30));

        quote.ExpiresAt.Should().Be(IssuedAt.AddSeconds(30));
    }

    [Fact]
    public void Constructor_rejects_the_same_currency_on_both_sides()
    {
        var act = () => new Quote(QuoteId.New(), Usd, Usd, 1m, 0.005m, IssuedAt, TimeSpan.FromSeconds(30));

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_rejects_a_non_positive_mid_market_rate(decimal rate)
    {
        var act = () => new Quote(QuoteId.New(), Usd, Pkr, rate, 0.005m, IssuedAt, TimeSpan.FromSeconds(30));

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void Convert_applies_the_customer_rate_and_rounds_to_the_target_currency_scale()
    {
        var quote = NewQuote(midMarketRate: 279.90m, spreadRate: 0.005m);

        var converted = quote.Convert(new Money(100m, Usd), asOf: IssuedAt);

        // 100 * 278.5005 = 27850.05
        converted.Should().Be(new Money(27_850.05m, Pkr));
    }

    [Fact]
    public void Convert_throws_when_the_quote_has_expired()
    {
        var quote = NewQuote(validFor: TimeSpan.FromSeconds(30));
        var afterExpiry = quote.ExpiresAt.AddSeconds(1);

        var act = () => quote.Convert(new Money(100m, Usd), afterExpiry);

        act.Should().Throw<QuoteExpiredException>();
    }

    [Fact]
    public void Convert_succeeds_at_the_exact_expiry_instant()
    {
        var quote = NewQuote(validFor: TimeSpan.FromSeconds(30));

        var act = () => quote.Convert(new Money(100m, Usd), quote.ExpiresAt);

        act.Should().NotThrow();
    }

    [Fact]
    public void Convert_throws_currency_mismatch_when_the_source_amount_is_in_the_wrong_currency()
    {
        var quote = NewQuote();

        var act = () => quote.Convert(new Money(100m, Pkr), IssuedAt);

        act.Should().Throw<CurrencyMismatchException>();
    }
}
