using CrossLedgerWeb.Domain.Exceptions;
using CrossLedgerWeb.Domain.Ledger;
using CrossLedgerWeb.Domain.ValueObjects;
using FluentAssertions;

namespace CrossLedgerWeb.Domain.Tests.Ledger;

public class LedgerInvariantsTests
{
    private static readonly Currency Usd = Currency.From("USD");
    private static readonly DateTimeOffset Now = DateTimeOffset.UtcNow;

    private static LedgerEntry Entry(LedgerDirection direction, decimal amount, TransferId transferId) =>
        new(LedgerEntryId.New(), transferId, WalletId.New(), direction, new Money(amount, Usd), Now);

    [Fact]
    public void Balanced_entries_pass_without_throwing()
    {
        var transferId = TransferId.New();
        var entries = new[]
        {
            Entry(LedgerDirection.Debit, 100m, transferId),
            Entry(LedgerDirection.Credit, 100m, transferId),
        };

        var act = () => LedgerInvariants.AssertZeroSumPerCurrency(entries);

        act.Should().NotThrow();
    }

    [Fact]
    public void Unbalanced_entries_for_a_currency_throw_ledger_imbalance()
    {
        var transferId = TransferId.New();
        var entries = new[]
        {
            Entry(LedgerDirection.Debit, 100m, transferId),
            Entry(LedgerDirection.Credit, 99.99m, transferId),
        };

        var act = () => LedgerInvariants.AssertZeroSumPerCurrency(entries);

        act.Should().Throw<LedgerImbalanceException>();
    }

    [Fact]
    public void Each_currency_is_checked_independently()
    {
        var pkr = Currency.From("PKR");
        var transferId = TransferId.New();
        var entries = new[]
        {
            Entry(LedgerDirection.Debit, 100m, transferId),
            Entry(LedgerDirection.Credit, 100m, transferId),
            new LedgerEntry(LedgerEntryId.New(), transferId, WalletId.New(), LedgerDirection.Debit, new Money(500m, pkr), Now),
        };

        var act = () => LedgerInvariants.AssertZeroSumPerCurrency(entries);

        act.Should().Throw<LedgerImbalanceException>().Which.Currency.Should().Be(pkr);
    }
}
