using CrossLedger.Domain.Exceptions;
using CrossLedger.Domain.ValueObjects;
using FluentAssertions;

namespace CrossLedger.Domain.Tests.ValueObjects;

public class MoneyTests
{
    private static readonly Currency Usd = Currency.From("USD");
    private static readonly Currency Pkr = Currency.From("PKR");
    private static readonly Currency Jpy = Currency.From("JPY");
    private static readonly Currency Kwd = Currency.From("KWD");

    [Fact]
    public void Constructor_accepts_an_amount_within_the_currency_scale()
    {
        var money = new Money(10.50m, Usd);

        money.Amount.Should().Be(10.50m);
    }

    [Fact]
    public void Constructor_throws_when_amount_has_more_precision_than_the_currency_allows()
    {
        var act = () => new Money(10.505m, Usd);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_rejects_fractional_amounts_for_a_zero_decimal_currency()
    {
        var act = () => new Money(100.5m, Jpy);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_accepts_three_decimal_places_for_a_three_decimal_currency()
    {
        var money = new Money(1.234m, Kwd);

        money.Amount.Should().Be(1.234m);
    }

    [Fact]
    public void Zero_produces_a_zero_amount_in_the_given_currency()
    {
        var money = Money.Zero(Usd);

        money.Amount.Should().Be(0m);
        money.Currency.Should().Be(Usd);
    }

    [Fact]
    public void Addition_of_the_same_currency_sums_the_amounts()
    {
        var result = new Money(10.00m, Usd) + new Money(5.50m, Usd);

        result.Amount.Should().Be(15.50m);
    }

    [Fact]
    public void Subtraction_of_the_same_currency_subtracts_the_amounts()
    {
        var result = new Money(10.00m, Usd) - new Money(3.25m, Usd);

        result.Amount.Should().Be(6.75m);
    }

    [Fact]
    public void Negation_flips_the_sign_and_keeps_the_currency()
    {
        var result = -new Money(10.00m, Usd);

        result.Amount.Should().Be(-10.00m);
        result.Currency.Should().Be(Usd);
    }

    [Fact]
    public void Addition_across_different_currencies_throws_currency_mismatch()
    {
        var act = () => new Money(10.00m, Usd) + new Money(10.00m, Pkr);

        act.Should().Throw<CurrencyMismatchException>();
    }

    [Fact]
    public void Subtraction_across_different_currencies_throws_currency_mismatch()
    {
        var act = () => new Money(10.00m, Usd) - new Money(10.00m, Pkr);

        act.Should().Throw<CurrencyMismatchException>();
    }

    [Fact]
    public void Comparison_across_different_currencies_throws_currency_mismatch()
    {
        var act = () => new Money(10.00m, Usd) > new Money(10.00m, Pkr);

        act.Should().Throw<CurrencyMismatchException>();
    }

    [Theory]
    [InlineData(10.00, 5.00, true, false)]
    [InlineData(5.00, 10.00, false, true)]
    [InlineData(5.00, 5.00, false, false)]
    public void Comparison_operators_order_amounts_of_the_same_currency(
        decimal leftAmount, decimal rightAmount, bool expectedGreaterThan, bool expectedLessThan)
    {
        var left = new Money(leftAmount, Usd);
        var right = new Money(rightAmount, Usd);

        (left > right).Should().Be(expectedGreaterThan);
        (left < right).Should().Be(expectedLessThan);
    }

    [Fact]
    public void Equal_amount_and_currency_are_equal_and_hash_consistently()
    {
        var a = new Money(10.00m, Usd);
        var b = new Money(10.00m, Usd);

        a.Should().Be(b);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void ToString_formats_using_the_currency_decimal_scale()
    {
        new Money(10m, Usd).ToString().Should().Be("10.00 USD");
        new Money(100m, Jpy).ToString().Should().Be("100 JPY");
        new Money(1.234m, Kwd).ToString().Should().Be("1.234 KWD");
    }

    [Fact]
    public void Repeated_addition_never_drifts_due_to_floating_point_error()
    {
        var total = Money.Zero(Usd);
        for (var i = 0; i < 10; i++)
            total += new Money(0.10m, Usd);

        total.Amount.Should().Be(1.00m);
    }
}
