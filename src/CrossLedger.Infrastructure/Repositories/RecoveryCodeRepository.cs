using CrossLedger.Application.Abstractions;
using CrossLedger.Domain.Auth;
using CrossLedger.Domain.ValueObjects;
using CrossLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CrossLedger.Infrastructure.Repositories;

public sealed class RecoveryCodeRepository : IRecoveryCodeRepository
{
    private readonly CrossLedgerDbContext _db;

    public RecoveryCodeRepository(CrossLedgerDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<RecoveryCode>> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken) =>
        await _db.RecoveryCodes.Where(c => c.UserId == userId).ToListAsync(cancellationToken);

    public void AddRange(IEnumerable<RecoveryCode> codes) => _db.RecoveryCodes.AddRange(codes);
}
