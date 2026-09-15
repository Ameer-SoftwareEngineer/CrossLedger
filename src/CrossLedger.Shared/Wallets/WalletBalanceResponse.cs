namespace CrossLedger.Shared.Wallets;

public sealed record WalletBalanceResponse(Guid WalletId, decimal Amount, string Currency);
