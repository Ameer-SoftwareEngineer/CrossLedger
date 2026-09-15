using System.Net;
using CrossLedger.Domain.ValueObjects;
using CrossLedger.Infrastructure.Fx;
using CrossLedger.Integration.Tests.TestSupport;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace CrossLedger.Integration.Tests.Fx;

public class ExchangeRateApiProviderTests
{
    private static readonly Currency Usd = Currency.From("USD");
    private static readonly Currency Pkr = Currency.From("PKR");
    private static readonly Uri BaseAddress = new("https://v6.exchangerate-api.com/");

    private static ExchangeRateApiProvider CreateProvider(FakeHttpMessageHandler handler)
    {
        var httpClient = FakeHttpMessageHandler.CreateClient(handler, BaseAddress);
        var options = Options.Create(new ExchangeRateApiOptions { ApiKey = "test-key" });
        return new ExchangeRateApiProvider(httpClient, options);
    }

    [Fact]
    public async Task Maps_a_successful_response_into_an_exchange_rate_reading()
    {
        const string json = """
            {
              "result": "success",
              "base_code": "USD",
              "target_code": "PKR",
              "conversion_rate": 278.50,
              "time_last_update_unix": 1893456000
            }
            """;
        var provider = CreateProvider(FakeHttpMessageHandler.ReturningJson(HttpStatusCode.OK, json));

        var reading = await provider.GetRateAsync(Usd, Pkr, CancellationToken.None);

        reading.From.Should().Be(Usd);
        reading.To.Should().Be(Pkr);
        reading.MidMarketRate.Should().Be(278.50m);
        reading.IsStale.Should().BeFalse();
        reading.AsOf.Should().Be(DateTimeOffset.FromUnixTimeSeconds(1893456000));
    }

    [Fact]
    public async Task Throws_when_the_provider_reports_an_error_result()
    {
        const string json = """{ "result": "error", "error-type": "unsupported-code" }""";
        var provider = CreateProvider(FakeHttpMessageHandler.ReturningJson(HttpStatusCode.OK, json));

        var act = () => provider.GetRateAsync(Usd, Pkr, CancellationToken.None);

        await act.Should().ThrowAsync<ExchangeRateProviderRejectedException>().WithMessage("*unsupported-code*");
    }
}
