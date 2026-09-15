using CrossLedgerWeb.Application.Abstractions;
using CrossLedgerWeb.Application.Exceptions;
using CrossLedgerWeb.Application.Fx;
using CrossLedgerWeb.Domain.ValueObjects;
using CrossLedgerWeb.Infrastructure.Fx;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;

namespace CrossLedgerWeb.Integration.Tests.Fx;

public class CachingExchangeRateProviderTests
{
    private static readonly Currency Usd = Currency.From("USD");
    private static readonly Currency Pkr = Currency.From("PKR");

    private readonly Mock<IExchangeRateProvider> _inner = new();
    private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());

    private CachingExchangeRateProvider CreateProvider() => new(_inner.Object, _cache);

    [Fact]
    public async Task Returns_and_caches_a_fresh_reading_on_success()
    {
        var reading = new ExchangeRateReading(Usd, Pkr, 278.50m, DateTimeOffset.UtcNow, IsStale: false);
        _inner.Setup(x => x.GetRateAsync(Usd, Pkr, It.IsAny<CancellationToken>())).ReturnsAsync(reading);
        var provider = CreateProvider();

        var result = await provider.GetRateAsync(Usd, Pkr, CancellationToken.None);

        result.Should().Be(reading);
    }

    [Fact]
    public async Task Serves_the_last_cached_reading_marked_stale_when_the_inner_provider_fails()
    {
        var fresh = new ExchangeRateReading(Usd, Pkr, 278.50m, DateTimeOffset.UtcNow, IsStale: false);
        _inner.SetupSequence(x => x.GetRateAsync(Usd, Pkr, It.IsAny<CancellationToken>()))
            .ReturnsAsync(fresh)
            .ThrowsAsync(new HttpRequestException("all providers down"));
        var provider = CreateProvider();
        await provider.GetRateAsync(Usd, Pkr, CancellationToken.None); // primes the cache

        var result = await provider.GetRateAsync(Usd, Pkr, CancellationToken.None);

        result.IsStale.Should().BeTrue();
        result.MidMarketRate.Should().Be(fresh.MidMarketRate);
        result.AsOf.Should().Be(fresh.AsOf);
    }

    [Fact]
    public async Task Throws_when_the_inner_provider_fails_and_nothing_was_ever_cached()
    {
        _inner.Setup(x => x.GetRateAsync(Usd, Pkr, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("all providers down"));
        var provider = CreateProvider();

        var act = () => provider.GetRateAsync(Usd, Pkr, CancellationToken.None);

        await act.Should().ThrowAsync<ExchangeRateUnavailableException>();
    }
}
