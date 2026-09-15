using CrossLedgerWeb.Domain.ValueObjects;

namespace CrossLedgerWeb.Application.Wallets;

public sealed record CreateWalletResult(WalletId WalletId, UserId OwnerId, Currency Currency);
