using System.Net;
using CrossLedger.Domain.ValueObjects;
using CrossLedger.Infrastructure.Fx;
using CrossLedger.Integration.Tests.TestSupport;
using FluentAssertions;

namespace CrossLedger.Integration.Tests.Fx;

public class FrankfurterProviderTests
{
    private static readonly Currency Usd = Currency.From("USD");
    private static readonly Currency Pkr = Currency.From("PKR");
    private static readonly Uri BaseAddress = new("https://api.frankfurter.app/");

    private static FrankfurterProvider CreateProvider(FakeHttpMessageHandler handler) =>
        new(FakeHttpMessageHandler.CreateClient(handler, BaseAddress));

    [Fact]
    public async Task Maps_a_successful_response_into_an_exchange_rate_reading()
    {
        const string json = """
            {
              "amount": 1.0,
              "base": "USD",
              "date": "2026-09-12",
              "rates": { "PKR": 278.5 }
            }
            """;
        var provider = CreateProvider(FakeHttpMessageHandler.ReturningJson(HttpStatusCode.OK, json));

        var reading = await provider.GetRateAsync(Usd, Pkr, CancellationToken.None);

        reading.From.Should().Be(Usd);
        reading.To.Should().Be(Pkr);
        reading.MidMarketRate.Should().Be(278.5m);
        reading.IsStale.Should().BeFalse();
        reading.AsOf.Should().Be(new DateTimeOffset(2026, 9, 12, 0, 0, 0, TimeSpan.Zero));
    }

    [Fact]
    public async Task Throws_when_the_target_currency_is_missing_from_the_response()
    {
        const string json = """{ "amount": 1.0, "base": "USD", "date": "2026-09-12", "rates": {} }""";
        var provider = CreateProvider(FakeHttpMessageHandler.ReturningJson(HttpStatusCode.OK, json));

        var act = () => provider.GetRateAsync(Usd, Pkr, CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
