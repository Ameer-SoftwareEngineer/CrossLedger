using CrossLedgerWeb.Application.Abstractions;
using CrossLedgerWeb.Domain.Auth;
using CrossLedgerWeb.Domain.ValueObjects;
using CrossLedgerWeb.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CrossLedgerWeb.Infrastructure.Repositories;

public sealed class RecoveryCodeRepository : IRecoveryCodeRepository
{
    private readonly CrossLedgerWebDbContext _db;

    public RecoveryCodeRepository(CrossLedgerWebDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<RecoveryCode>> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken) =>
        await _db.RecoveryCodes.Where(c => c.UserId == userId).ToListAsync(cancellationToken);

    public void AddRange(IEnumerable<RecoveryCode> codes) => _db.RecoveryCodes.AddRange(codes);
}
