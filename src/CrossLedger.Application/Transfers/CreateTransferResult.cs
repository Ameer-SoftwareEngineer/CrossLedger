using CrossLedger.Domain.ValueObjects;

namespace CrossLedger.Application.Transfers;

public sealed record CreateTransferResult(
    TransferId TransferId,
    Money SourceAmount,
    Money TargetAmount,
    DateTimeOffset PostedAt);
