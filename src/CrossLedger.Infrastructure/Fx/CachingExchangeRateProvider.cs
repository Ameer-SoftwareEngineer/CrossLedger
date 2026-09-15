using CrossLedger.Application.Abstractions;
using CrossLedger.Application.Exceptions;
using CrossLedger.Application.Fx;
using CrossLedger.Domain.ValueObjects;
using Microsoft.Extensions.Caching.Memory;

namespace CrossLedger.Infrastructure.Fx;

/// <summary>
/// TTL 1 hour, stale-if-error (specification 2.5): every successful reading is cached
/// for an hour; when the inner provider chain fails entirely, the last cached reading is
/// served instead with <see cref="ExchangeRateReading.IsStale"/> set, rather than failing
/// the request outright. Only once nothing has ever been cached (or the cache entry has
/// aged out past its hour) does this propagate the failure.
/// </summary>
public sealed class CachingExchangeRateProvider : IExchangeRateProvider
{
    private static readonly TimeSpan CacheTtl = TimeSpan.FromHours(1);

    private readonly IExchangeRateProvider _inner;
    private readonly IMemoryCache _cache;

    public CachingExchangeRateProvider(IExchangeRateProvider inner, IMemoryCache cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public async Task<ExchangeRateReading> GetRateAsync(Currency from, Currency to, CancellationToken cancellationToken)
    {
        var key = CacheKey(from, to);

        try
        {
            var reading = await _inner.GetRateAsync(from, to, cancellationToken);
            _cache.Set(key, reading, CacheTtl);
            return reading;
        }
        catch (Exception ex)
        {
            if (_cache.TryGetValue(key, out ExchangeRateReading? stale) && stale is not null)
                return stale with { IsStale = true };

            throw new ExchangeRateUnavailableException(from, to, ex);
        }
    }

    private static string CacheKey(Currency from, Currency to) => $"fx:{from.Code}:{to.Code}";
}
