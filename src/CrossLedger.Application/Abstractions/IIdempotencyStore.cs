namespace CrossLedger.Application.Abstractions;

/// <summary>Backed by a unique database constraint on the idempotency key in
/// production (specification section 2.3): a repeated key must return the original
/// stored response instead of re-executing the request.</summary>
public interface IIdempotencyStore
{
    Task<string?> FindResponseAsync(string idempotencyKey, CancellationToken cancellationToken);

    Task SaveResponseAsync(string idempotencyKey, string serializedResponse, CancellationToken cancellationToken);
}
