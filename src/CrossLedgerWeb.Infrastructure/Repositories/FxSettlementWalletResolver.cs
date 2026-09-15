using CrossLedgerWeb.Application.Abstractions;
using CrossLedgerWeb.Domain.ValueObjects;
using CrossLedgerWeb.Domain.Wallets;
using CrossLedgerWeb.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CrossLedgerWeb.Infrastructure.Repositories;

public sealed class FxSettlementWalletResolver : IFxSettlementWalletResolver
{
    private readonly CrossLedgerWebDbContext _db;

    public FxSettlementWalletResolver(CrossLedgerWebDbContext db)
    {
        _db = db;
    }

    public async Task<Wallet> GetSettlementWalletAsync(Currency currency, CancellationToken cancellationToken)
    {
        var wallet = await _db.Wallets
            .Include(w => w.Entries)
            .FirstOrDefaultAsync(w => w.Kind == WalletKind.SystemClearing && w.Currency == currency, cancellationToken);

        return wallet ?? throw new SettlementWalletNotConfiguredException(currency);
    }
}
