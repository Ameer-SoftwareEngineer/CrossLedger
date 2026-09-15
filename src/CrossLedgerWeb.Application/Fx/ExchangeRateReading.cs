using CrossLedgerWeb.Domain.ValueObjects;

namespace CrossLedgerWeb.Application.Fx;

/// <summary>
/// A mid-market rate as read from a provider — deliberately not the same type as
/// <see cref="CrossLedgerWeb.Domain.Fx.Quote"/>, which is the spread-adjusted rate locked
/// for a customer. This is the raw input a Quote gets built from.
/// </summary>
/// <param name="IsStale">True when this reading came from cache after every live
/// provider failed (specification 2.5's stale-if-error path), not from a fresh call.</param>
public sealed record ExchangeRateReading(Currency From, Currency To, decimal MidMarketRate, DateTimeOffset AsOf, bool IsStale);
