using CrossLedger.Domain.ValueObjects;

namespace CrossLedger.Application.Payments;

public sealed record PayoutRequest(
    TransferId TransferId,
    Corridor Corridor,
    Money Amount,
    string IdempotencyKey,
    RoutingPreference Preference);
