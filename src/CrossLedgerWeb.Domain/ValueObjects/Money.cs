using System.Globalization;
using CrossLedgerWeb.Domain.Exceptions;

namespace CrossLedgerWeb.Domain.ValueObjects;

/// <summary>
/// An exact decimal amount in a specific <see cref="Currency"/>. Arithmetic and
/// comparison across two different currencies throws <see cref="CurrencyMismatchException"/>
/// instead of silently producing a meaningless number — cross-currency conversion must
/// go through an explicit FX quote, never raw arithmetic.
/// </summary>
public readonly record struct Money : IComparable<Money>
{
    public decimal Amount { get; }
    public Currency Currency { get; }

    public Money(decimal amount, Currency currency)
    {
        // Reject precision the currency can't represent instead of silently rounding it
        // away — a hidden rounding step here could mask a bug and break the ledger's
        // zero-sum invariant.
        var rounded = Math.Round(amount, currency.DecimalPlaces, MidpointRounding.ToEven);
        if (rounded != amount)
        {
            throw new ArgumentException(
                $"Amount {amount} has more precision than {currency.Code} allows ({currency.DecimalPlaces} decimal place(s)).",
                nameof(amount));
        }

        Amount = amount;
        Currency = currency;
    }

    public static Money Zero(Currency currency) => new(0m, currency);

    public static Money operator +(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return new Money(left.Amount + right.Amount, left.Currency);
    }

    public static Money operator -(Money left, Money right)
    {
        EnsureSameCurrency(left, right);
        return new Money(left.Amount - right.Amount, left.Currency);
    }

    public static Money operator -(Money value) => new(-value.Amount, value.Currency);

    public static bool operator >(Money left, Money right) => left.CompareTo(right) > 0;
    public static bool operator <(Money left, Money right) => left.CompareTo(right) < 0;
    public static bool operator >=(Money left, Money right) => left.CompareTo(right) >= 0;
    public static bool operator <=(Money left, Money right) => left.CompareTo(right) <= 0;

    public int CompareTo(Money other)
    {
        EnsureSameCurrency(this, other);
        return Amount.CompareTo(other.Amount);
    }

    private static void EnsureSameCurrency(Money left, Money right)
    {
        if (left.Currency != right.Currency)
            throw new CurrencyMismatchException(left.Currency, right.Currency);
    }

    public override string ToString() =>
        $"{Amount.ToString($"F{Currency.DecimalPlaces}", CultureInfo.InvariantCulture)} {Currency.Code}";
}
