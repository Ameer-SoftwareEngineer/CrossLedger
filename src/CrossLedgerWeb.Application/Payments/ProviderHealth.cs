using CrossLedgerWeb.Domain.Payments;

namespace CrossLedgerWeb.Application.Payments;

public enum ProviderHealthStatus
{
    Healthy,
    Degraded,
    Unavailable,
}

public sealed record ProviderHealth(ProviderCode ProviderCode, ProviderHealthStatus Status, DateTimeOffset CheckedAt);
