namespace CrossLedger.Shared.Wallets;

/// <summary>Wire contract for POST /api/v1/wallets. Deliberately plain primitives, not
/// Domain value objects - CrossLedger.Shared is referenced by the Blazor client too,
/// which has no business knowing about WalletId or Currency.</summary>
public sealed record CreateWalletRequest(Guid OwnerId, string Currency);
