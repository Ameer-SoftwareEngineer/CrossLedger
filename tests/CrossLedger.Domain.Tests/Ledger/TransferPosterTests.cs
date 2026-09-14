using CrossLedger.Domain.Exceptions;
using CrossLedger.Domain.Fx;
using CrossLedger.Domain.Ledger;
using CrossLedger.Domain.ValueObjects;
using CrossLedger.Domain.Wallets;
using FluentAssertions;

namespace CrossLedger.Domain.Tests.Ledger;

public class TransferPosterTests
{
    private static readonly Currency Usd = Currency.From("USD");
    private static readonly Currency Pkr = Currency.From("PKR");
    private static readonly DateTimeOffset Now = DateTimeOffset.UtcNow;

    private static Wallet CustomerWallet(Currency currency, decimal openingBalance = 0m)
    {
        var wallet = new Wallet(WalletId.New(), UserId.New(), currency);
        if (openingBalance > 0)
            wallet.Credit(new Money(openingBalance, currency), TransferId.New(), Now);
        return wallet;
    }

    private static Wallet ClearingWallet(Currency currency) =>
        new(WalletId.New(), UserId.New(), currency, WalletKind.SystemClearing);

    [Fact]
    public void Posting_the_specification_worked_example_produces_the_four_documented_entries()
    {
        // Transfer: 100 USD -> PKR @ rate 278.50 (specification section 2.1)
        var sourceWallet = CustomerWallet(Usd, openingBalance: 100m);
        var targetWallet = CustomerWallet(Pkr);
        var fxSettlementUsd = ClearingWallet(Usd);
        var fxSettlementPkr = ClearingWallet(Pkr);
        var transferId = TransferId.New();

        var entries = TransferPoster.Post(
            transferId,
            sourceWallet, fxSettlementUsd, fxSettlementPkr, targetWallet,
            sourceAmount: new Money(100m, Usd),
            targetAmount: new Money(27_850m, Pkr),
            postedAt: Now);

        entries.Should().HaveCount(4);
        entries.Should().OnlyContain(e => e.TransferId == transferId);

        sourceWallet.Balance.Amount.Should().Be(0m);
        fxSettlementUsd.Balance.Amount.Should().Be(100m);
        fxSettlementPkr.Balance.Amount.Should().Be(-27_850m);
        targetWallet.Balance.Amount.Should().Be(27_850m);

        LedgerInvariants.AssertZeroSumPerCurrency(entries);
    }

    [Fact]
    public void Posting_through_a_quote_converts_at_the_quotes_customer_rate()
    {
        var sourceWallet = CustomerWallet(Usd, openingBalance: 100m);
        var targetWallet = CustomerWallet(Pkr);
        var fxSettlementUsd = ClearingWallet(Usd);
        var fxSettlementPkr = ClearingWallet(Pkr);
        var quote = new Quote(QuoteId.New(), Usd, Pkr, midMarketRate: 279.90m, spreadRate: 0.005m, Now, TimeSpan.FromSeconds(30));

        var entries = TransferPoster.Post(
            TransferId.New(), quote,
            sourceWallet, fxSettlementUsd, fxSettlementPkr, targetWallet,
            sourceAmount: new Money(100m, Usd),
            postedAt: Now);

        entries.Should().HaveCount(4);
        targetWallet.Balance.Should().Be(quote.Convert(new Money(100m, Usd), Now));
        LedgerInvariants.AssertZeroSumPerCurrency(entries);
    }

    [Fact]
    public void Posting_through_an_expired_quote_throws_and_posts_nothing()
    {
        var sourceWallet = CustomerWallet(Usd, openingBalance: 100m);
        var targetWallet = CustomerWallet(Pkr);
        var fxSettlementUsd = ClearingWallet(Usd);
        var fxSettlementPkr = ClearingWallet(Pkr);
        var quote = new Quote(QuoteId.New(), Usd, Pkr, midMarketRate: 279.90m, spreadRate: 0.005m, Now, TimeSpan.FromSeconds(30));
        var afterExpiry = quote.ExpiresAt.AddSeconds(1);

        var act = () => TransferPoster.Post(
            TransferId.New(), quote,
            sourceWallet, fxSettlementUsd, fxSettlementPkr, targetWallet,
            sourceAmount: new Money(100m, Usd),
            postedAt: afterExpiry);

        act.Should().Throw<QuoteExpiredException>();
        sourceWallet.Entries.Should().HaveCount(1); // only the opening credit from test setup
        targetWallet.Entries.Should().BeEmpty();
    }

    [Fact]
    public void Insufficient_source_funds_throws_and_posts_nothing_to_any_wallet()
    {
        var sourceWallet = CustomerWallet(Usd, openingBalance: 50m);
        var targetWallet = CustomerWallet(Pkr);
        var fxSettlementUsd = ClearingWallet(Usd);
        var fxSettlementPkr = ClearingWallet(Pkr);

        var act = () => TransferPoster.Post(
            TransferId.New(),
            sourceWallet, fxSettlementUsd, fxSettlementPkr, targetWallet,
            sourceAmount: new Money(100m, Usd),
            targetAmount: new Money(27_850m, Pkr),
            postedAt: Now);

        act.Should().Throw<InsufficientFundsException>();

        sourceWallet.Entries.Should().HaveCount(1); // only the opening credit from test setup
        fxSettlementUsd.Entries.Should().BeEmpty();
        fxSettlementPkr.Entries.Should().BeEmpty();
        targetWallet.Entries.Should().BeEmpty();
    }

    [Fact]
    public void Consecutive_transfers_keep_every_wallet_balanced_against_its_own_entries()
    {
        var sourceWallet = CustomerWallet(Usd, openingBalance: 300m);
        var targetWallet = CustomerWallet(Pkr);
        var fxSettlementUsd = ClearingWallet(Usd);
        var fxSettlementPkr = ClearingWallet(Pkr);

        var postedEntries = new List<LedgerEntry>();
        for (var i = 0; i < 3; i++)
        {
            postedEntries.AddRange(TransferPoster.Post(
                TransferId.New(),
                sourceWallet, fxSettlementUsd, fxSettlementPkr, targetWallet,
                sourceAmount: new Money(100m, Usd),
                targetAmount: new Money(27_850m, Pkr),
                postedAt: Now));
        }

        sourceWallet.Balance.Amount.Should().Be(0m);
        targetWallet.Balance.Amount.Should().Be(83_550m);

        // Only the entries produced by the transfers themselves must be zero-sum — the
        // wallets' full history also includes the test's own opening-balance funding
        // entry, which is an unmatched credit by construction (real funding would post
        // a matching debit against a funding-source clearing wallet).
        var act = () => LedgerInvariants.AssertZeroSumPerCurrency(postedEntries);
        act.Should().NotThrow();
    }
}
