using CrossLedgerWeb.Application.Abstractions;
using CrossLedgerWeb.Domain.Payments;
using CrossLedgerWeb.Infrastructure.Persistence;
using CrossLedgerWeb.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace CrossLedgerWeb.Infrastructure.Repositories;

public sealed class ProcessedWebhookEventStore : IProcessedWebhookEventStore
{
    private readonly CrossLedgerWebDbContext _db;

    public ProcessedWebhookEventStore(CrossLedgerWebDbContext db)
    {
        _db = db;
    }

    public Task<bool> HasBeenProcessedAsync(ProviderCode providerCode, string eventId, CancellationToken cancellationToken) =>
        _db.ProcessedWebhookEvents.AnyAsync(
            e => e.ProviderCode == providerCode.ToString() && e.EventId == eventId,
            cancellationToken);

    public void MarkProcessed(ProviderCode providerCode, string eventId, DateTimeOffset processedAt) =>
        _db.ProcessedWebhookEvents.Add(new ProcessedWebhookEvent
        {
            ProviderCode = providerCode.ToString(),
            EventId = eventId,
            ProcessedAt = processedAt,
        });
}
