using CrossLedgerWeb.Domain.ValueObjects;
using FluentAssertions;

namespace CrossLedgerWeb.Domain.Tests.ValueObjects;

public class CurrencyTests
{
    [Theory]
    [InlineData("USD", 2)]
    [InlineData("PKR", 2)]
    [InlineData("EUR", 2)]
    [InlineData("JPY", 0)]
    [InlineData("KRW", 0)]
    [InlineData("KWD", 3)]
    [InlineData("BHD", 3)]
    public void From_returns_the_correct_iso4217_minor_unit_scale(string code, int expectedDecimalPlaces)
    {
        var currency = Currency.From(code);

        currency.DecimalPlaces.Should().Be(expectedDecimalPlaces);
    }

    [Fact]
    public void From_normalizes_lowercase_and_surrounding_whitespace()
    {
        var currency = Currency.From(" usd ");

        currency.Code.Should().Be("USD");
    }

    [Fact]
    public void From_throws_for_an_unsupported_code()
    {
        var act = () => Currency.From("XXX");

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void From_throws_for_a_null_or_blank_code(string? code)
    {
        var act = () => Currency.From(code!);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Two_currencies_with_the_same_code_are_equal()
    {
        Currency.From("USD").Should().Be(Currency.From("USD"));
    }
}
