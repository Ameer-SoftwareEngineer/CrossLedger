using CrossLedgerWeb.Application.Abstractions;
using CrossLedgerWeb.Domain.Auth;
using CrossLedgerWeb.Domain.ValueObjects;
using CrossLedgerWeb.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CrossLedgerWeb.Infrastructure.Repositories;

public sealed class UsedTotpCodeRepository : IUsedTotpCodeRepository
{
    private readonly CrossLedgerWebDbContext _db;

    public UsedTotpCodeRepository(CrossLedgerWebDbContext db)
    {
        _db = db;
    }

    public Task<bool> IsActiveAsync(UserId userId, string code, DateTimeOffset asOf, CancellationToken cancellationToken) =>
        _db.UsedTotpCodes.AnyAsync(c => c.UserId == userId && c.Code == code && c.ExpiresAt > asOf, cancellationToken);

    public void Add(UsedTotpCode usedCode) => _db.UsedTotpCodes.Add(usedCode);
}
