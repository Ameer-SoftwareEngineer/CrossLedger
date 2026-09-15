using CrossLedgerWeb.Domain.ValueObjects;

namespace CrossLedgerWeb.Application.Payments;

public sealed record ProviderQuote(
    ProviderCode ProviderCode,
    Money Amount,
    Money Fee,
    TimeSpan EstimatedSettlementTime);
