using CrossLedger.Application.Abstractions;
using CrossLedger.Domain.ValueObjects;
using CrossLedger.Domain.Wallets;
using CrossLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CrossLedger.Infrastructure.Repositories;

public sealed class WalletRepository : IWalletRepository
{
    private readonly CrossLedgerDbContext _db;

    public WalletRepository(CrossLedgerDbContext db)
    {
        _db = db;
    }

    public Task<Wallet?> GetByIdAsync(WalletId id, CancellationToken cancellationToken) =>
        _db.Wallets.Include(w => w.Entries).FirstOrDefaultAsync(w => w.Id == id, cancellationToken);

    public void Add(Wallet wallet) => _db.Wallets.Add(wallet);
}
