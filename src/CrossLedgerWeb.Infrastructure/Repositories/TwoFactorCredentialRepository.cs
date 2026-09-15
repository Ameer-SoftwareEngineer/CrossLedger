using CrossLedgerWeb.Application.Abstractions;
using CrossLedgerWeb.Domain.Auth;
using CrossLedgerWeb.Domain.ValueObjects;
using CrossLedgerWeb.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CrossLedgerWeb.Infrastructure.Repositories;

public sealed class TwoFactorCredentialRepository : ITwoFactorCredentialRepository
{
    private readonly CrossLedgerWebDbContext _db;

    public TwoFactorCredentialRepository(CrossLedgerWebDbContext db)
    {
        _db = db;
    }

    public Task<TwoFactorCredential?> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken) =>
        _db.TwoFactorCredentials.FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

    public void Add(TwoFactorCredential credential) => _db.TwoFactorCredentials.Add(credential);
}
