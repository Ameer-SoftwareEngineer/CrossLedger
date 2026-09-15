using CrossLedger.Shared.Auth;

namespace CrossLedger.Shared.Transfers;

/// <summary>Wire contract for POST /api/v1/transfers. The idempotency key travels as
/// the Idempotency-Key header (specification 2.3), not a body field.</summary>
public sealed record CreateTransferRequest(Guid QuoteId, Guid SourceWalletId, Guid TargetWalletId, decimal SourceAmount)
    : IStepUpAmountSource
{
    decimal IStepUpAmountSource.StepUpAmount => SourceAmount;
}
