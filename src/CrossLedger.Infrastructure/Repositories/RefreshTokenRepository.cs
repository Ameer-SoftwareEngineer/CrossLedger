using CrossLedger.Application.Abstractions;
using CrossLedger.Domain.Auth;
using CrossLedger.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CrossLedger.Infrastructure.Repositories;

public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly CrossLedgerDbContext _db;

    public RefreshTokenRepository(CrossLedgerDbContext db)
    {
        _db = db;
    }

    public Task<RefreshToken?> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken) =>
        _db.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

    public async Task<IReadOnlyList<RefreshToken>> GetFamilyAsync(Guid familyId, CancellationToken cancellationToken) =>
        await _db.RefreshTokens.Where(t => t.FamilyId == familyId).ToListAsync(cancellationToken);

    public void Add(RefreshToken refreshToken) => _db.RefreshTokens.Add(refreshToken);
}
