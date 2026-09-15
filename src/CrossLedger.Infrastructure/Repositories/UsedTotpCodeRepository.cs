using CrossLedger.Application.Abstractions;
using CrossLedger.Domain.Auth;
using CrossLedger.Domain.ValueObjects;
using CrossLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CrossLedger.Infrastructure.Repositories;

public sealed class UsedTotpCodeRepository : IUsedTotpCodeRepository
{
    private readonly CrossLedgerDbContext _db;

    public UsedTotpCodeRepository(CrossLedgerDbContext db)
    {
        _db = db;
    }

    public Task<bool> IsActiveAsync(UserId userId, string code, DateTimeOffset asOf, CancellationToken cancellationToken) =>
        _db.UsedTotpCodes.AnyAsync(c => c.UserId == userId && c.Code == code && c.ExpiresAt > asOf, cancellationToken);

    public void Add(UsedTotpCode usedCode) => _db.UsedTotpCodes.Add(usedCode);
}
