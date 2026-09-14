using CrossLedger.Domain.ValueObjects;
using CrossLedger.Domain.Wallets;

namespace CrossLedger.Domain.Ledger;

/// <summary>
/// Posts one cross-currency transfer as four balanced ledger entries:
/// debit the source wallet, credit the source-currency FX settlement account,
/// debit the target-currency FX settlement account, credit the target wallet.
/// Amounts are pre-computed by the caller (from an accepted FX quote) — this
/// service only enforces that the posting is atomic and balanced, never rates.
/// </summary>
public static class TransferPoster
{
    public static IReadOnlyList<LedgerEntry> Post(
        TransferId transferId,
        Wallet sourceWallet,
        Wallet fxSettlementSource,
        Wallet fxSettlementTarget,
        Wallet targetWallet,
        Money sourceAmount,
        Money targetAmount,
        DateTimeOffset postedAt)
    {
        // Evaluated in order; if the source debit fails (insufficient funds), the
        // remaining three postings never execute and no wallet is touched.
        var entries = new[]
        {
            sourceWallet.Debit(sourceAmount, transferId, postedAt),
            fxSettlementSource.Credit(sourceAmount, transferId, postedAt),
            fxSettlementTarget.Debit(targetAmount, transferId, postedAt),
            targetWallet.Credit(targetAmount, transferId, postedAt),
        };

        LedgerInvariants.AssertZeroSumPerCurrency(entries);

        return entries;
    }
}
