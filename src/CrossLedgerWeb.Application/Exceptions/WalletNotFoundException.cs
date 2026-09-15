using CrossLedgerWeb.Domain.ValueObjects;

namespace CrossLedgerWeb.Application.Exceptions;

public sealed class WalletNotFoundException : Exception
{
    public WalletId WalletId { get; }

    public WalletNotFoundException(WalletId walletId)
        : base($"Wallet {walletId} was not found.")
    {
        WalletId = walletId;
    }
}
