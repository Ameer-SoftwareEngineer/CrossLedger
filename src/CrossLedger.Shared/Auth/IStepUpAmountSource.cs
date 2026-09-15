namespace CrossLedger.Shared.Auth;

/// <summary>Implemented by request DTOs whose monetary amount decides whether a
/// threshold-scoped [RequireStepUp] (specification 6.2's "Transfer above threshold")
/// actually applies - lets the filter read the amount without reflecting over every
/// possible request shape.</summary>
public interface IStepUpAmountSource
{
    decimal StepUpAmount { get; }
}
