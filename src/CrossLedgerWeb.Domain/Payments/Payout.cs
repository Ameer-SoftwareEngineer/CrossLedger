using CrossLedgerWeb.Domain.Exceptions;
using CrossLedgerWeb.Domain.ValueObjects;

namespace CrossLedgerWeb.Domain.Payments;

/// <summary>
/// The lifecycle of a single external payout attempt (specification 5.5) - distinct
/// from <see cref="Ledger.TransferId"/>'s internal wallet-to-wallet movement. Transitions
/// are enforced here, not by convention: an invalid one throws rather than silently
/// corrupting the payout's recorded state.
/// </summary>
public sealed class Payout
{
    private static readonly IReadOnlyDictionary<PayoutState, PayoutState[]> AllowedTransitions =
        new Dictionary<PayoutState, PayoutState[]>
        {
            [PayoutState.Quoted] = [PayoutState.Reserved],
            [PayoutState.Reserved] = [PayoutState.Submitted],
            [PayoutState.Submitted] = [PayoutState.ProviderFailed, PayoutState.Processing],
            [PayoutState.ProviderFailed] = [PayoutState.Submitted, PayoutState.PendingManual],
            [PayoutState.PendingManual] = [],
            [PayoutState.Processing] = [PayoutState.Settled],
            [PayoutState.Settled] = [],
            [PayoutState.Reversed] = [],
        };

    public PayoutId Id { get; }
    public TransferId TransferId { get; }
    public PayoutState State { get; private set; }

    public Payout(PayoutId id, TransferId transferId)
    {
        Id = id;
        TransferId = transferId;
        State = PayoutState.Quoted;
    }

    public void TransitionTo(PayoutState newState)
    {
        // "Any state -> REVERSED posts compensating ledger entries" (5.5) - the one
        // exception to the table below. Reversed itself is terminal: no double-reversal.
        if (newState == PayoutState.Reversed)
        {
            if (State == PayoutState.Reversed)
                throw new InvalidPayoutTransitionException(State, newState);

            State = newState;
            return;
        }

        if (!AllowedTransitions.TryGetValue(State, out var allowed) || !allowed.Contains(newState))
            throw new InvalidPayoutTransitionException(State, newState);

        State = newState;
    }
}
