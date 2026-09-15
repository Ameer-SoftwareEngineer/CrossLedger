using CrossLedgerWeb.Application.Abstractions;
using CrossLedgerWeb.Application.Fx;
using CrossLedgerWeb.Domain.ValueObjects;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace CrossLedgerWeb.Infrastructure.Fx;

/// <summary>
/// Implements the resilience chain from specification 2.5: the primary provider gets
/// three exponential-backoff retries wrapped in a circuit breaker (so a sustained outage
/// stops hammering it), and if that whole attempt still fails, the call falls over to
/// the secondary provider. Must be registered as a singleton - the circuit breaker only
/// means anything if its failure/success counters persist across calls.
/// </summary>
public sealed class ResilientExchangeRateProvider : IExchangeRateProvider
{
    private readonly IExchangeRateProvider _primary;
    private readonly IExchangeRateProvider _fallback;
    private readonly ResiliencePipeline<ExchangeRateReading> _primaryPipeline;

    public ResilientExchangeRateProvider(IExchangeRateProvider primary, IExchangeRateProvider fallback)
    {
        _primary = primary;
        _fallback = fallback;

        _primaryPipeline = new ResiliencePipelineBuilder<ExchangeRateReading>()
            .AddRetry(new RetryStrategyOptions<ExchangeRateReading>
            {
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential,
                Delay = TimeSpan.FromMilliseconds(200),
                // A bad API key or an unsupported pair fails identically on every
                // attempt - retrying it just burns several seconds of backoff before
                // reaching the fallback anyway. Only genuinely transient failures
                // (timeouts, connection resets, 5xx) are worth retrying.
                ShouldHandle = new PredicateBuilder<ExchangeRateReading>()
                    .Handle<Exception>(ex => ex is not ExchangeRateProviderRejectedException),
            })
            .AddCircuitBreaker(new CircuitBreakerStrategyOptions<ExchangeRateReading>
            {
                FailureRatio = 0.5,
                MinimumThroughput = 4,
                SamplingDuration = TimeSpan.FromSeconds(30),
                BreakDuration = TimeSpan.FromSeconds(15),
            })
            .Build();
    }

    public async Task<ExchangeRateReading> GetRateAsync(Currency from, Currency to, CancellationToken cancellationToken)
    {
        try
        {
            return await _primaryPipeline.ExecuteAsync(
                async token => await _primary.GetRateAsync(from, to, token),
                cancellationToken);
        }
        catch (Exception)
        {
            // Deterministic failures (unsupported pair, bad response) and exhausted
            // retries/open-circuit both land here - either way, try the other provider.
            // If this was actually a cancellation, the fallback call below observes the
            // same token and throws OperationCanceledException itself.
            return await _fallback.GetRateAsync(from, to, cancellationToken);
        }
    }
}
