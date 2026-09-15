using System.Net.Http.Json;
using CrossLedgerWeb.Application.Payments;
using CrossLedgerWeb.Domain.ValueObjects;
using CrossLedgerWeb.Providers.Common;
using Microsoft.Extensions.Options;

namespace CrossLedgerWeb.Providers.Rapyd;

/// <summary>
/// Secondary payout provider for alternate corridors (specification 5.1) - selected by
/// the routing engine when it scores better than Airwallex for a given corridor, or as
/// the failover when Airwallex is unhealthy. Rapyd's real API signs every request (not
/// just webhooks) with a per-call HMAC scheme involving a salt and timestamp; this
/// implementation covers webhook verification with the shared HmacSignatureVerifier and
/// leaves per-request request-signing as a follow-up once a real sandbox account exists
/// to verify the exact scheme against - see AirwallexProvider's doc comment for why.
/// </summary>
public sealed class RapydProvider : IPaymentProvider
{
    private readonly HttpClient _httpClient;
    private readonly IOptions<RapydOptions> _options;

    public RapydProvider(HttpClient httpClient, IOptions<RapydOptions> options)
    {
        _httpClient = httpClient;
        _options = options;
    }

    public ProviderCode Code => ProviderCode.Rapyd;

    public bool Supports(Corridor corridor) =>
        _options.Value.SupportedDestinationCountries.Contains(corridor.DestinationCountry, StringComparer.OrdinalIgnoreCase);

    public async Task<ProviderQuote> QuoteAsync(PayoutRequest request, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "v1/payouts/quote",
            new
            {
                payout_currency = request.Corridor.TargetCurrency.Code,
                beneficiary_country = request.Corridor.DestinationCountry,
                sender_amount = request.Amount.Amount,
                sender_currency = request.Corridor.SourceCurrency.Code,
            },
            cancellationToken);

        response.EnsureSuccessStatusCode();
        var quote = await response.Content.ReadFromJsonAsync<RapydQuoteResponse>(cancellationToken)
            ?? throw new InvalidOperationException("Rapyd returned an empty quote response.");

        var fee = new Money(quote.Fee, request.Amount.Currency);
        return new ProviderQuote(Code, request.Amount, fee, TimeSpan.FromHours(quote.EstimatedDeliveryHours));
    }

    public async Task<PayoutResult> ExecuteAsync(PayoutRequest request, CancellationToken cancellationToken)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "v1/payouts",
            new
            {
                merchant_reference_id = request.IdempotencyKey,
                payout_amount = request.Amount.Amount,
                payout_currency = request.Corridor.TargetCurrency.Code,
                beneficiary_country = request.Corridor.DestinationCountry,
            },
            cancellationToken);

        if (!response.IsSuccessStatusCode)
            return new PayoutResult(Code, PayoutOutcome.Rejected, ProviderReference: null, FailureReason: await response.Content.ReadAsStringAsync(cancellationToken));

        var payout = await response.Content.ReadFromJsonAsync<RapydPayoutResponse>(cancellationToken)
            ?? throw new InvalidOperationException("Rapyd returned an empty payout response.");

        return new PayoutResult(Code, PayoutOutcome.Accepted, payout.Id, FailureReason: null);
    }

    public bool VerifySignature(string payload, string signature, string secret) =>
        HmacSignatureVerifier.Verify(payload, signature, secret);

    public async Task<ProviderHealth> CheckHealthAsync(CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync("v1/account", cancellationToken);
            var status = response.IsSuccessStatusCode ? ProviderHealthStatus.Healthy : ProviderHealthStatus.Degraded;
            return new ProviderHealth(Code, status, DateTimeOffset.UtcNow);
        }
        catch (HttpRequestException)
        {
            return new ProviderHealth(Code, ProviderHealthStatus.Unavailable, DateTimeOffset.UtcNow);
        }
    }

    private sealed class RapydQuoteResponse
    {
        public decimal Fee { get; set; }
        public double EstimatedDeliveryHours { get; set; } = 48;
    }

    private sealed class RapydPayoutResponse
    {
        public string Id { get; set; } = string.Empty;
    }
}
