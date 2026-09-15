using CrossLedger.Domain.Payments;

namespace CrossLedger.Domain.Exceptions;

public sealed class InvalidPayoutTransitionException : DomainException
{
    public PayoutState From { get; }
    public PayoutState To { get; }

    public InvalidPayoutTransitionException(PayoutState from, PayoutState to)
        : base($"Cannot transition a payout from {from} to {to}.")
    {
        From = from;
        To = to;
    }
}
