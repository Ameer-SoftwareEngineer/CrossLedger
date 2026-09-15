using CrossLedger.Domain.ValueObjects;

namespace CrossLedger.Application.Wallets;

public sealed record CreateWalletResult(WalletId WalletId, UserId OwnerId, Currency Currency);
