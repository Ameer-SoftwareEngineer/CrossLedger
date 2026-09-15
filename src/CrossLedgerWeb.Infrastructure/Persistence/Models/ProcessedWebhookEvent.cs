namespace CrossLedgerWeb.Infrastructure.Persistence.Models;

/// <summary>Pure persistence record - not a domain concept, just the backing store for
/// IProcessedWebhookEventStore's dedup-on-event-id guarantee (specification 5.4).</summary>
public sealed class ProcessedWebhookEvent
{
    public string ProviderCode { get; set; } = default!;
    public string EventId { get; set; } = default!;
    public DateTimeOffset ProcessedAt { get; set; }
}
