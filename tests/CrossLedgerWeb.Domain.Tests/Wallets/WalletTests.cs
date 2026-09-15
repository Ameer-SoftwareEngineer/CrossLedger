using CrossLedgerWeb.Domain.Exceptions;
using CrossLedgerWeb.Domain.Ledger;
using CrossLedgerWeb.Domain.ValueObjects;
using CrossLedgerWeb.Domain.Wallets;
using FluentAssertions;

namespace CrossLedgerWeb.Domain.Tests.Wallets;

public class WalletTests
{
    private static readonly Currency Usd = Currency.From("USD");
    private static readonly Currency Pkr = Currency.From("PKR");
    private static readonly DateTimeOffset Now = DateTimeOffset.UtcNow;

    private static Wallet NewWallet(Currency? currency = null, WalletKind kind = WalletKind.Customer) =>
        new(WalletId.New(), UserId.New(), currency ?? Usd, kind);

    [Fact]
    public void A_new_wallet_has_a_zero_balance()
    {
        var wallet = NewWallet();

        wallet.Balance.Should().Be(Money.Zero(Usd));
    }

    [Fact]
    public void Credit_increases_the_derived_balance()
    {
        var wallet = NewWallet();

        wallet.Credit(new Money(50m, Usd), TransferId.New(), Now);

        wallet.Balance.Amount.Should().Be(50m);
    }

    [Fact]
    public void Debit_decreases_the_derived_balance_when_funds_are_available()
    {
        var wallet = NewWallet();
        wallet.Credit(new Money(50m, Usd), TransferId.New(), Now);

        wallet.Debit(new Money(20m, Usd), TransferId.New(), Now);

        wallet.Balance.Amount.Should().Be(30m);
    }

    [Fact]
    public void Balance_always_equals_the_sum_of_every_posted_entrys_signed_amount()
    {
        var wallet = NewWallet();
        wallet.Credit(new Money(100m, Usd), TransferId.New(), Now);
        wallet.Debit(new Money(40m, Usd), TransferId.New(), Now);
        wallet.Credit(new Money(10m, Usd), TransferId.New(), Now);

        var expected = wallet.Entries.Aggregate(Money.Zero(Usd), (sum, e) => sum + e.SignedAmount);
        wallet.Balance.Should().Be(expected);
    }

    [Fact]
    public void A_customer_wallet_cannot_be_debited_below_zero()
    {
        var wallet = NewWallet(kind: WalletKind.Customer);
        wallet.Credit(new Money(10m, Usd), TransferId.New(), Now);

        var act = () => wallet.Debit(new Money(10.01m, Usd), TransferId.New(), Now);

        act.Should().Throw<InsufficientFundsException>();
    }

    [Fact]
    public void An_insufficient_funds_rejection_leaves_the_balance_unchanged()
    {
        var wallet = NewWallet(kind: WalletKind.Customer);
        wallet.Credit(new Money(10m, Usd), TransferId.New(), Now);

        try { wallet.Debit(new Money(999m, Usd), TransferId.New(), Now); } catch (InsufficientFundsException) { }

        wallet.Balance.Amount.Should().Be(10m);
        wallet.Entries.Should().HaveCount(1);
    }

    [Fact]
    public void A_system_clearing_wallet_may_be_debited_into_a_negative_balance()
    {
        var wallet = NewWallet(Pkr, WalletKind.SystemClearing);

        wallet.Debit(new Money(27_850m, Pkr), TransferId.New(), Now);

        wallet.Balance.Amount.Should().Be(-27_850m);
    }

    [Fact]
    public void Posting_in_a_different_currency_than_the_wallet_throws_currency_mismatch()
    {
        var wallet = NewWallet(Usd);

        var act = () => wallet.Credit(new Money(10m, Pkr), TransferId.New(), Now);

        act.Should().Throw<CurrencyMismatchException>();
    }

    [Fact]
    public void Entries_are_append_only()
    {
        var wallet = NewWallet();

        wallet.Credit(new Money(10m, Usd), TransferId.New(), Now);
        wallet.Credit(new Money(5m, Usd), TransferId.New(), Now);

        wallet.Entries.Should().HaveCount(2);
        wallet.Entries.Should().OnlyHaveUniqueItems(e => e.Id);
    }
}
