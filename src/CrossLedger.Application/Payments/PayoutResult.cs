namespace CrossLedger.Application.Payments;

public enum PayoutOutcome
{
    Accepted,
    Rejected,
}

public sealed record PayoutResult(ProviderCode ProviderCode, PayoutOutcome Outcome, string? ProviderReference, string? FailureReason);
