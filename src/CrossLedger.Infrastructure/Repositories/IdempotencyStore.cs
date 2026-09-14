using CrossLedger.Application.Abstractions;
using CrossLedger.Infrastructure.Persistence;
using CrossLedger.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace CrossLedger.Infrastructure.Repositories;

public sealed class IdempotencyStore : IIdempotencyStore
{
    private readonly CrossLedgerDbContext _db;
    private readonly IClock _clock;

    public IdempotencyStore(CrossLedgerDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    public async Task<string?> FindResponseAsync(string idempotencyKey, CancellationToken cancellationToken)
    {
        var record = await _db.IdempotencyRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Key == idempotencyKey, cancellationToken);

        return record?.ResponsePayload;
    }

    /// <summary>Only stages the record - UnitOfWorkBehavior's single SaveChanges call at
    /// the end of the pipeline is what actually commits it, together with whatever the
    /// handler staged, so both land in one transaction.</summary>
    public Task SaveResponseAsync(string idempotencyKey, string serializedResponse, CancellationToken cancellationToken)
    {
        _db.IdempotencyRecords.Add(new IdempotencyRecord
        {
            Key = idempotencyKey,
            ResponsePayload = serializedResponse,
            CreatedAt = _clock.UtcNow,
        });

        return Task.CompletedTask;
    }
}
