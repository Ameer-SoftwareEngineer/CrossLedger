using CrossLedgerWeb.Domain.ValueObjects;

namespace CrossLedgerWeb.Domain.Exceptions;

public sealed class InsufficientFundsException : DomainException
{
    public WalletId WalletId { get; }
    public Money AvailableBalance { get; }
    public Money RequestedAmount { get; }

    public InsufficientFundsException(WalletId walletId, Money availableBalance, Money requestedAmount)
        : base($"Wallet {walletId} has insufficient funds: balance {availableBalance}, requested {requestedAmount}.")
    {
        WalletId = walletId;
        AvailableBalance = availableBalance;
        RequestedAmount = requestedAmount;
    }
}
