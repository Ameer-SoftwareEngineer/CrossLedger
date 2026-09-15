using CrossLedger.Application.Abstractions;
using CrossLedger.Application.Payments;
using CrossLedger.Domain.ValueObjects;
using CrossLedger.Infrastructure.Persistence;
using CrossLedger.Infrastructure.Persistence.Models;

namespace CrossLedger.Infrastructure.Repositories;

public sealed class RoutingAuditLog : IRoutingAuditLog
{
    private readonly CrossLedgerDbContext _db;
    private readonly IClock _clock;

    public RoutingAuditLog(CrossLedgerDbContext db, IClock clock)
    {
        _db = db;
        _clock = clock;
    }

    /// <summary>Only stages the records - like IdempotencyStore, the pipeline's single
    /// UnitOfWorkBehavior SaveChanges call is what actually commits them.</summary>
    public Task RecordAsync(TransferId transferId, IReadOnlyList<ScoredProviderQuote> rankedQuotes, CancellationToken cancellationToken)
    {
        var recordedAt = _clock.UtcNow;

        for (var rank = 0; rank < rankedQuotes.Count; rank++)
        {
            var scored = rankedQuotes[rank];
            _db.RoutingDecisions.Add(new RoutingDecisionRecord
            {
                Id = Guid.NewGuid(),
                TransferId = transferId.Value,
                ProviderCode = scored.Quote.ProviderCode.ToString(),
                Rank = rank,
                Score = scored.Score,
                FeeAmount = scored.Quote.Fee.Amount,
                FeeCurrency = scored.Quote.Fee.Currency.Code,
                EstimatedSettlementMinutes = scored.Quote.EstimatedSettlementTime.TotalMinutes,
                RecordedAt = recordedAt,
            });
        }

        return Task.CompletedTask;
    }
}
