namespace CrossLedger.Application.Abstractions;

/// <summary>Marks a MediatR request as requiring idempotency-key protection. Only
/// requests implementing this are intercepted by <c>IdempotencyBehavior</c> — plain
/// queries pass straight through.</summary>
public interface IIdempotentRequest
{
    string IdempotencyKey { get; }
}
