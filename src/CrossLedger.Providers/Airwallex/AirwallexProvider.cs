using System.Net.Http.Json;
using CrossLedger.Application.Payments;
using CrossLedger.Domain.ValueObjects;
using CrossLedger.Providers.Common;
using Microsoft.Extensions.Options;

namespace CrossLedger.Providers.Airwallex;

/// <summary>
/// Primary payout provider (specification 5.1): multi-currency accounts, international
/// transfers with conversion quotes, sandbox at api.sandbox.airwallex.com. Idempotency
/// via request_id; webhooks signed HMAC-SHA256 with an x-signature header.
///
/// Request/response shapes here follow Airwallex's public API documentation but have
/// NOT been exercised against a live sandbox call - that needs a real ClientId/ApiKey,
/// which this environment doesn't have. Treat the JSON contracts as a solid starting
/// point to verify against your own sandbox account, not as tested integration code -
/// unlike CachingExchangeRateProvider's chain, which was verified end to end.
/// </summary>
public sealed class AirwallexProvider : IPaymentProvider
{
    private readonly HttpClient _httpClient;
    private readonly IOptions<AirwallexOptions> _options;

    public AirwallexProvider(HttpClient httpClient, IOptions<AirwallexOptions> options)
    {
        _httpClient = httpClient;
        _options = options;
    }

    public ProviderCode Code => ProviderCode.Airwallex;

    public bool Supports(Corridor corridor) =>
        _options.Value.SupportedDestinationCountries.Contains(corridor.DestinationCountry, StringComparer.OrdinalIgnoreCase);

    public async Task<ProviderQuote> QuoteAsync(PayoutRequest request, CancellationToken cancellationToken)
    {
        await AuthenticateAsync(cancellationToken);

        var response = await _httpClient.PostAsJsonAsync(
            "api/v1/pa/payments/quote",
            new
            {
                source_currency = request.Corridor.SourceCurrency.Code,
                target_currency = request.Corridor.TargetCurrency.Code,
                amount = request.Amount.Amount,
            },
            cancellationToken);

        response.EnsureSuccessStatusCode();
        var quote = await response.Content.ReadFromJsonAsync<AirwallexQuoteResponse>(cancellationToken)
            ?? throw new InvalidOperationException("Airwallex returned an empty quote response.");

        var fee = new Money(quote.FeeAmount, request.Amount.Currency);
        return new ProviderQuote(Code, request.Amount, fee, TimeSpan.FromHours(quote.EstimatedHours));
    }

    public async Task<PayoutResult> ExecuteAsync(PayoutRequest request, CancellationToken cancellationToken)
    {
        await AuthenticateAsync(cancellationToken);

        var response = await _httpClient.PostAsJsonAsync(
            "api/v1/pa/payments/create",
            new
            {
                request_id = request.IdempotencyKey, // Airwallex's own idempotency key
                amount = request.Amount.Amount,
                source_currency = request.Corridor.SourceCurrency.Code,
                target_currency = request.Corridor.TargetCurrency.Code,
                destination_country = request.Corridor.DestinationCountry,
            },
            cancellationToken);

        if (!response.IsSuccessStatusCode)
            return new PayoutResult(Code, PayoutOutcome.Rejected, ProviderReference: null, FailureReason: await response.Content.ReadAsStringAsync(cancellationToken));

        var payment = await response.Content.ReadFromJsonAsync<AirwallexPaymentResponse>(cancellationToken)
            ?? throw new InvalidOperationException("Airwallex returned an empty payment response.");

        return new PayoutResult(Code, PayoutOutcome.Accepted, payment.Id, FailureReason: null);
    }

    public bool VerifySignature(string payload, string signature, string secret) =>
        HmacSignatureVerifier.Verify(payload, signature, secret);

    public async Task<ProviderHealth> CheckHealthAsync(CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync("api/v1/pa/accounts/status", cancellationToken);
            var status = response.IsSuccessStatusCode ? ProviderHealthStatus.Healthy : ProviderHealthStatus.Degraded;
            return new ProviderHealth(Code, status, DateTimeOffset.UtcNow);
        }
        catch (HttpRequestException)
        {
            return new ProviderHealth(Code, ProviderHealthStatus.Unavailable, DateTimeOffset.UtcNow);
        }
    }

    private async Task AuthenticateAsync(CancellationToken cancellationToken)
    {
        if (_httpClient.DefaultRequestHeaders.Authorization is not null)
            return;

        var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/authentication/login");
        request.Headers.Add("x-client-id", _options.Value.ClientId);
        request.Headers.Add("x-api-key", _options.Value.ApiKey);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var token = await response.Content.ReadFromJsonAsync<AirwallexLoginResponse>(cancellationToken)
            ?? throw new InvalidOperationException("Airwallex authentication returned an empty response.");

        _httpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token.Token);
    }

    private sealed class AirwallexLoginResponse
    {
        public string Token { get; set; } = string.Empty;
    }

    private sealed class AirwallexQuoteResponse
    {
        public decimal FeeAmount { get; set; }
        public double EstimatedHours { get; set; } = 24;
    }

    private sealed class AirwallexPaymentResponse
    {
        public string Id { get; set; } = string.Empty;
    }
}
