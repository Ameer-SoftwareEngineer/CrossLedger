using CrossLedger.Domain.Exceptions;
using CrossLedger.Domain.ValueObjects;

namespace CrossLedger.Domain.Fx;

/// <summary>
/// A rate locked for a short window (specification section 2.2). A transfer converts
/// money through an accepted <see cref="Quote"/>, never through a raw rate lookup at
/// execution time — this is what stops a user viewing a rate, waiting, and executing
/// against a now-stale favourable price.
/// </summary>
public sealed class Quote
{
    public QuoteId Id { get; }
    public Currency FromCurrency { get; }
    public Currency ToCurrency { get; }
    public decimal MidMarketRate { get; }

    /// <summary>The platform's margin, e.g. 0.005m for 0.50%. Applied against the
    /// mid-market rate to produce <see cref="CustomerRate"/> and recorded as platform
    /// revenue, never silently absorbed.</summary>
    public decimal SpreadRate { get; }

    /// <summary>The rate actually applied to the customer's conversion — always worse
    /// than <see cref="MidMarketRate"/> by exactly <see cref="SpreadRate"/>.</summary>
    public decimal CustomerRate { get; }

    public DateTimeOffset IssuedAt { get; }
    public DateTimeOffset ExpiresAt { get; }

    public Quote(
        QuoteId id,
        Currency fromCurrency,
        Currency toCurrency,
        decimal midMarketRate,
        decimal spreadRate,
        DateTimeOffset issuedAt,
        TimeSpan validFor)
    {
        if (fromCurrency == toCurrency)
            throw new ArgumentException("A quote requires two different currencies.", nameof(toCurrency));

        if (midMarketRate <= 0)
            throw new ArgumentOutOfRangeException(nameof(midMarketRate), midMarketRate, "The mid-market rate must be positive.");

        if (spreadRate is < 0 or >= 1)
            throw new ArgumentOutOfRangeException(nameof(spreadRate), spreadRate, "The spread rate must be between 0 (inclusive) and 1 (exclusive).");

        if (validFor <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(validFor), validFor, "A quote must be valid for a positive duration.");

        Id = id;
        FromCurrency = fromCurrency;
        ToCurrency = toCurrency;
        MidMarketRate = midMarketRate;
        SpreadRate = spreadRate;
        CustomerRate = Math.Round(midMarketRate * (1 - spreadRate), 8, MidpointRounding.ToEven);
        IssuedAt = issuedAt;
        ExpiresAt = issuedAt + validFor;
    }

    public bool IsExpired(DateTimeOffset asOf) => asOf > ExpiresAt;

    /// <summary>Converts an amount in <see cref="FromCurrency"/> into <see cref="ToCurrency"/>
    /// at <see cref="CustomerRate"/>. Throws <see cref="QuoteExpiredException"/> rather than
    /// silently applying a stale rate.</summary>
    public Money Convert(Money sourceAmount, DateTimeOffset asOf)
    {
        if (sourceAmount.Currency != FromCurrency)
            throw new CurrencyMismatchException(FromCurrency, sourceAmount.Currency);

        if (IsExpired(asOf))
            throw new QuoteExpiredException(Id, ExpiresAt, asOf);

        var converted = Math.Round(sourceAmount.Amount * CustomerRate, ToCurrency.DecimalPlaces, MidpointRounding.ToEven);
        return new Money(converted, ToCurrency);
    }
}
