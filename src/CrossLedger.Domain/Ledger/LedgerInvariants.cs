using CrossLedger.Domain.Exceptions;
using CrossLedger.Domain.ValueObjects;

namespace CrossLedger.Domain.Ledger;

/// <summary>
/// The invariant that makes the ledger trustworthy: entries for any given currency,
/// across any set of postings, must always sum to zero. <see cref="TransferPoster"/>
/// checks this after every posting; it is also asserted independently in tests so a
/// regression in the posting logic is caught immediately rather than discovered later
/// as a real money discrepancy.
/// </summary>
public static class LedgerInvariants
{
    public static void AssertZeroSumPerCurrency(IEnumerable<LedgerEntry> entries)
    {
        foreach (var group in entries.GroupBy(e => e.Amount.Currency))
        {
            var currency = group.Key;
            var sum = group.Aggregate(Money.Zero(currency), (total, entry) => total + entry.SignedAmount);

            if (sum.Amount != 0)
                throw new LedgerImbalanceException(currency, sum);
        }
    }
}
