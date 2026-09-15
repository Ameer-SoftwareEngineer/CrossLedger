using CrossLedger.Domain.ValueObjects;

namespace CrossLedger.Application.Payments;

public sealed record ProviderQuote(
    ProviderCode ProviderCode,
    Money Amount,
    Money Fee,
    TimeSpan EstimatedSettlementTime);
