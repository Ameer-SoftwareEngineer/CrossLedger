using CrossLedger.Application.Abstractions;
using CrossLedger.Application.Fx;
using CrossLedger.Domain.ValueObjects;
using CrossLedger.Infrastructure.Fx;
using FluentAssertions;
using Moq;

namespace CrossLedger.Integration.Tests.Fx;

public class ResilientExchangeRateProviderTests
{
    private static readonly Currency Usd = Currency.From("USD");
    private static readonly Currency Pkr = Currency.From("PKR");

    private readonly Mock<IExchangeRateProvider> _primary = new();
    private readonly Mock<IExchangeRateProvider> _fallback = new();

    [Fact]
    public async Task Returns_the_primarys_reading_when_it_succeeds()
    {
        var reading = new ExchangeRateReading(Usd, Pkr, 278.50m, DateTimeOffset.UtcNow, IsStale: false);
        _primary.Setup(x => x.GetRateAsync(Usd, Pkr, It.IsAny<CancellationToken>())).ReturnsAsync(reading);
        var provider = new ResilientExchangeRateProvider(_primary.Object, _fallback.Object);

        var result = await provider.GetRateAsync(Usd, Pkr, CancellationToken.None);

        result.Should().Be(reading);
        _fallback.Verify(x => x.GetRateAsync(It.IsAny<Currency>(), It.IsAny<Currency>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Falls_over_to_the_secondary_provider_once_the_primary_is_exhausted()
    {
        _primary.Setup(x => x.GetRateAsync(Usd, Pkr, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("primary unavailable"));
        var fallbackReading = new ExchangeRateReading(Usd, Pkr, 279.10m, DateTimeOffset.UtcNow, IsStale: false);
        _fallback.Setup(x => x.GetRateAsync(Usd, Pkr, It.IsAny<CancellationToken>())).ReturnsAsync(fallbackReading);
        var provider = new ResilientExchangeRateProvider(_primary.Object, _fallback.Object);

        var result = await provider.GetRateAsync(Usd, Pkr, CancellationToken.None);

        result.Should().Be(fallbackReading);
    }

    [Fact]
    public async Task Retries_the_primary_before_falling_back()
    {
        _primary.Setup(x => x.GetRateAsync(Usd, Pkr, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("transient failure"));
        _fallback.Setup(x => x.GetRateAsync(Usd, Pkr, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExchangeRateReading(Usd, Pkr, 279.10m, DateTimeOffset.UtcNow, IsStale: false));
        var provider = new ResilientExchangeRateProvider(_primary.Object, _fallback.Object);

        await provider.GetRateAsync(Usd, Pkr, CancellationToken.None);

        // MaxRetryAttempts: 3 plus the initial attempt = 4 calls to the primary.
        _primary.Verify(x => x.GetRateAsync(Usd, Pkr, It.IsAny<CancellationToken>()), Times.Exactly(4));
    }

    [Fact]
    public async Task Propagates_the_fallbacks_failure_when_both_providers_fail()
    {
        _primary.Setup(x => x.GetRateAsync(Usd, Pkr, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("primary down"));
        _fallback.Setup(x => x.GetRateAsync(Usd, Pkr, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("fallback down"));
        var provider = new ResilientExchangeRateProvider(_primary.Object, _fallback.Object);

        var act = () => provider.GetRateAsync(Usd, Pkr, CancellationToken.None);

        await act.Should().ThrowAsync<HttpRequestException>().WithMessage("fallback down");
    }
}
