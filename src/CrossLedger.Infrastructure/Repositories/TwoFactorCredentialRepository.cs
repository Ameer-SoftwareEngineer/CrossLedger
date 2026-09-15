using CrossLedger.Application.Abstractions;
using CrossLedger.Domain.Auth;
using CrossLedger.Domain.ValueObjects;
using CrossLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CrossLedger.Infrastructure.Repositories;

public sealed class TwoFactorCredentialRepository : ITwoFactorCredentialRepository
{
    private readonly CrossLedgerDbContext _db;

    public TwoFactorCredentialRepository(CrossLedgerDbContext db)
    {
        _db = db;
    }

    public Task<TwoFactorCredential?> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken) =>
        _db.TwoFactorCredentials.FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

    public void Add(TwoFactorCredential credential) => _db.TwoFactorCredentials.Add(credential);
}
