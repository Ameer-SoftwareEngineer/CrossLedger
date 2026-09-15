namespace CrossLedgerWeb.Shared.Wallets;

public sealed record WalletBalanceResponse(Guid WalletId, decimal Amount, string Currency);
