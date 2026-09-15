using CrossLedgerWeb.Domain.ValueObjects;

namespace CrossLedgerWeb.Application.Transfers;

public sealed record CreateTransferResult(
    TransferId TransferId,
    Money SourceAmount,
    Money TargetAmount,
    DateTimeOffset PostedAt);
