using System.Net.Http.Json;
using CrossLedgerWeb.Application.Abstractions;
using CrossLedgerWeb.Application.Fx;
using CrossLedgerWeb.Domain.ValueObjects;
using CrossLedgerWeb.Infrastructure.Fx.Dtos;
using Microsoft.Extensions.Options;

namespace CrossLedgerWeb.Infrastructure.Fx;

/// <summary>Primary FX rate provider - 160+ currencies, any base currency
/// (specification 3.5). Talks to ExchangeRate-API's pair-conversion endpoint and maps
/// its response into <see cref="ExchangeRateReading"/> before returning.</summary>
public sealed class ExchangeRateApiProvider : IExchangeRateProvider
{
    private readonly HttpClient _httpClient;
    private readonly IOptions<ExchangeRateApiOptions> _options;

    public ExchangeRateApiProvider(HttpClient httpClient, IOptions<ExchangeRateApiOptions> options)
    {
        _httpClient = httpClient;
        _options = options;
    }

    public async Task<ExchangeRateReading> GetRateAsync(Currency from, Currency to, CancellationToken cancellationToken)
    {
        var response = await _httpClient.GetFromJsonAsync<ExchangeRateApiPairResponse>(
            $"v6/{_options.Value.ApiKey}/pair/{from.Code}/{to.Code}", cancellationToken)
            ?? throw new InvalidOperationException("ExchangeRate-API returned an empty response.");

        if (!string.Equals(response.Result, "success", StringComparison.OrdinalIgnoreCase) || response.ConversionRate is null)
            throw new ExchangeRateProviderRejectedException($"ExchangeRate-API request failed: {response.ErrorType ?? "unknown error"}.");

        var asOf = DateTimeOffset.FromUnixTimeSeconds(response.TimeLastUpdateUnix);

        return new ExchangeRateReading(from, to, response.ConversionRate.Value, asOf, IsStale: false);
    }
}
