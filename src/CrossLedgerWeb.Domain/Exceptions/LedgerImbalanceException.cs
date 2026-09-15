using CrossLedgerWeb.Domain.ValueObjects;

namespace CrossLedgerWeb.Domain.Exceptions;

/// <summary>
/// Thrown when a set of ledger entries for a currency does not sum to zero. This should
/// never happen through normal posting — <see cref="Ledger.TransferPoster"/> only ever
/// produces balanced entries — so hitting this indicates a bug, not a business-rule
/// rejection.
/// </summary>
public sealed class LedgerImbalanceException : DomainException
{
    public Currency Currency { get; }
    public Money Sum { get; }

    public LedgerImbalanceException(Currency currency, Money sum)
        : base($"Ledger entries for {currency.Code} do not sum to zero (sum = {sum}).")
    {
        Currency = currency;
        Sum = sum;
    }
}
