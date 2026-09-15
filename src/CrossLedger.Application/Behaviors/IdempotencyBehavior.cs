using System.Text.Json;
using CrossLedger.Application.Abstractions;
using CrossLedger.Application.Serialization;
using MediatR;

namespace CrossLedger.Application.Behaviors;

/// <summary>Implements the idempotency-key contract from specification 2.3: a request
/// that implements <see cref="IIdempotentRequest"/> and repeats its key returns the
/// originally stored response instead of executing the handler again.</summary>
public sealed class IdempotencyBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    // Money and Currency both need custom converters - see those types for why
    // System.Text.Json's default reflection-based (de)serialization silently corrupts
    // them instead of throwing. Any response carrying a Domain value object through
    // this cache must serialize with these options, not JsonSerializer's bare defaults.
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        Converters = { new CurrencyJsonConverter(), new MoneyJsonConverter() },
    };

    private readonly IIdempotencyStore _store;

    public IdempotencyBehavior(IIdempotencyStore store)
    {
        _store = store;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not IIdempotentRequest idempotent)
            return await next().ConfigureAwait(false);

        var existing = await _store.FindResponseAsync(idempotent.IdempotencyKey, cancellationToken).ConfigureAwait(false);
        if (existing is not null)
            return JsonSerializer.Deserialize<TResponse>(existing, SerializerOptions)!;

        var response = await next().ConfigureAwait(false);

        var serialized = JsonSerializer.Serialize(response, SerializerOptions);
        await _store.SaveResponseAsync(idempotent.IdempotencyKey, serialized, cancellationToken).ConfigureAwait(false);

        return response;
    }
}
