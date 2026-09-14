using CrossLedger.Application.Abstractions;
using CrossLedger.Domain.ValueObjects;
using CrossLedger.Domain.Wallets;
using CrossLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CrossLedger.Infrastructure.Repositories;

public sealed class FxSettlementWalletResolver : IFxSettlementWalletResolver
{
    private readonly CrossLedgerDbContext _db;

    public FxSettlementWalletResolver(CrossLedgerDbContext db)
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
