using CrossLedgerWeb.Domain.ValueObjects;

namespace CrossLedgerWeb.Application.Payments;

public sealed record PayoutRequest(
    TransferId TransferId,
    Corridor Corridor,
    Money Amount,
    string IdempotencyKey,
    RoutingPreference Preference);
