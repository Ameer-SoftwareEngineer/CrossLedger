namespace CrossLedger.Shared.Wallets;

public sealed record CreateWalletResponse(Guid WalletId, Guid OwnerId, string Currency);
