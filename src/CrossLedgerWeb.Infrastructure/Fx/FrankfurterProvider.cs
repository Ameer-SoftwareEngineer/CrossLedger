using System.Net.Http.Json;
using CrossLedgerWeb.Application.Abstractions;
using CrossLedgerWeb.Application.Fx;
using CrossLedgerWeb.Domain.ValueObjects;
using CrossLedgerWeb.Infrastructure.Fx.Dtos;

namespace CrossLedgerWeb.Infrastructure.Fx;

/// <summary>Fallback FX rate provider - open source, unlimited, no API key; ECB-sourced
/// daily rates (specification 3.5).</summary>
public sealed class FrankfurterProvider : IExchangeRateProvider
{
    private readonly HttpClient _httpClient;

    public FrankfurterProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ExchangeRateReading> GetRateAsync(Currency from, Currency to, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetFromJsonAsync<FrankfurterLatestResponse>(
            $"latest?from={from.Code}&to={to.Code}", cancellationToken)
            ?? throw new InvalidOperationException("Frankfurter returned an empty response.");

        if (!response.Rates.TryGetValue(to.Code, out var rate))
            throw new ExchangeRateProviderRejectedException($"Frankfurter did not return a rate for {from.Code}/{to.Code}.");

        var asOf = new DateTimeOffset(response.Date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

        return new ExchangeRateReading(from, to, rate, asOf, IsStale: false);
    }
}
