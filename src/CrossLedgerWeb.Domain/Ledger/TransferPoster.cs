using CrossLedgerWeb.Domain.Fx;
using CrossLedgerWeb.Domain.ValueObjects;
using CrossLedgerWeb.Domain.Wallets;

namespace CrossLedgerWeb.Domain.Ledger;

/// <summary>
/// Posts one cross-currency transfer as four balanced ledger entries:
/// debit the source wallet, credit the source-currency FX settlement account,
/// debit the target-currency FX settlement account, credit the target wallet.
/// </summary>
public static class TransferPoster
{
    /// <summary>Posts a transfer through an accepted <see cref="Quote"/> rather than a
    /// raw amount — the quote itself rejects a stale rate via <see cref="Quote.Convert"/>,
    /// so an expired quote can never reach the ledger.</summary>
    public static IReadOnlyList<LedgerEntry> Post(
        TransferId transferId,
        Quote quote,
        Wallet sourceWallet,
        Wallet fxSettlementSource,
        Wallet fxSettlementTarget,
        Wallet targetWallet,
        Money sourceAmount,
        DateTimeOffset postedAt)
    {
        var targetAmount = quote.Convert(sourceAmount, postedAt);

        return Post(
            transferId, sourceWallet, fxSettlementSource, fxSettlementTarget, targetWallet,
            sourceAmount, targetAmount, postedAt);
    }

    /// <summary>Posts a transfer with pre-computed amounts. Prefer the <see cref="Quote"/>
    /// overload in application code; this exists for callers that have already validated
    /// a rate through some other path (e.g. tests).</summary>
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
