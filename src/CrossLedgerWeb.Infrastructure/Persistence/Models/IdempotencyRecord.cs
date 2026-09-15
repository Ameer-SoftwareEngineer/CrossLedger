namespace CrossLedgerWeb.Infrastructure.Persistence.Models;

/// <summary>Pure persistence record - not a domain concept, just the backing store for
/// IIdempotencyStore's unique-key-per-request guarantee (specification 2.3).</summary>
public sealed class IdempotencyRecord
{
    public string Key { get; set; } = default!;
    public string ResponsePayload { get; set; } = default!;
    public DateTimeOffset CreatedAt { get; set; }
}
