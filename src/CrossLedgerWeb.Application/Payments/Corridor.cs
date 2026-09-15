using CrossLedgerWeb.Domain.ValueObjects;

namespace CrossLedgerWeb.Application.Payments;

/// <summary>The source currency, target currency and destination country a payout
/// needs to move through - the unit a provider declares support for
/// (<see cref="IPaymentProvider.Supports"/>).</summary>
public sealed record Corridor(Currency SourceCurrency, Currency TargetCurrency, string DestinationCountry);
