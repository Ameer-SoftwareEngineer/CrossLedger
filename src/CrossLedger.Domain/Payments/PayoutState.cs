namespace CrossLedger.Domain.Payments;

/// <summary>Transfer lifecycle states from specification 5.5.</summary>
public enum PayoutState
{
    Quoted,
    Reserved,
    Submitted,
    ProviderFailed,
    PendingManual,
    Processing,
    Settled,
    Reversed,
}
