using System.Text;
using System.Text.Json;
using CrossLedgerWeb.Api.Webhooks;
using CrossLedgerWeb.Application.Payments;
using CrossLedgerWeb.Domain.Payments;
using CrossLedgerWeb.Providers.Airwallex;
using CrossLedgerWeb.Providers.Rapyd;
using CrossLedgerWeb.Providers.Simulated;
using CrossLedgerWeb.Providers.Stripe;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CrossLedgerWeb.Api.Controllers;

/// <summary>
/// Receives payout status callbacks from a provider (specification 5.4). Signature
/// verification happens here, not in Application - this is the layer that actually holds
/// each provider's webhook secret (via its own Options class). Never authenticated with
/// our own bearer tokens - a provider cannot present one - so [AllowAnonymous]; the
/// signature check is what stands in for authentication (matches how Stripe, Airwallex
/// etc. actually protect their webhook endpoints).
/// </summary>
[ApiController]
[Route("api/v1/webhooks/payments")]
[AllowAnonymous]
public sealed class PaymentWebhooksController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IEnumerable<IPaymentProvider> _providers;
    private readonly IOptions<AirwallexOptions> _airwallexOptions;
    private readonly IOptions<RapydOptions> _rapydOptions;
    private readonly IOptions<StripeOptions> _stripeOptions;
    private readonly IOptions<SimulatedProviderOptions> _simulatedOptions;

    public PaymentWebhooksController(
        IMediator mediator,
        IEnumerable<IPaymentProvider> providers,
        IOptions<AirwallexOptions> airwallexOptions,
        IOptions<RapydOptions> rapydOptions,
        IOptions<StripeOptions> stripeOptions,
        IOptions<SimulatedProviderOptions> simulatedOptions)
    {
        _mediator = mediator;
        _providers = providers;
        _airwallexOptions = airwallexOptions;
        _rapydOptions = rapydOptions;
        _stripeOptions = stripeOptions;
        _simulatedOptions = simulatedOptions;
    }

    [HttpPost("{providerCodeRoute}")]
    public async Task<IActionResult> Receive(string providerCodeRoute, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<ProviderCode>(providerCodeRoute, ignoreCase: true, out var providerCode))
            return NotFound();

        var provider = _providers.FirstOrDefault(p => p.Code == providerCode);
        if (provider is null)
            return NotFound();

        Request.EnableBuffering();
        string rawBody;
        using (var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true))
        {
            rawBody = await reader.ReadToEndAsync(cancellationToken);
        }
        Request.Body.Position = 0;

        // Stripe signs with its own "t=<ts>,v1=<hex>" header; every other provider here
        // uses a bare HMAC hex signature in a custom header (specification 5.1's
        // "x-signature" for Airwallex - reused generically for Rapyd/Simulated too, since
        // neither has a real inbound webhook contract verified against a live sandbox -
        // see PayoutWebhookEnvelope's own doc comment).
        var signatureHeaderName = providerCode == ProviderCode.Stripe ? "Stripe-Signature" : "X-Signature";
        var signature = Request.Headers[signatureHeaderName].ToString();
        var secret = ResolveWebhookSecret(providerCode);

        if (string.IsNullOrEmpty(signature) || !provider.VerifySignature(rawBody, signature, secret))
            return Unauthorized();

        PayoutWebhookEnvelope? envelope;
        try
        {
            envelope = JsonSerializer.Deserialize<PayoutWebhookEnvelope>(rawBody, JsonOptions);
        }
        catch (JsonException)
        {
            return BadRequest(new { code = "INVALID_PAYLOAD", message = "Webhook body is not valid JSON." });
        }

        if (envelope is null
            || string.IsNullOrWhiteSpace(envelope.EventId)
            || string.IsNullOrWhiteSpace(envelope.PayoutReference)
            || !Enum.TryParse<PayoutWebhookEventType>(envelope.Status, ignoreCase: true, out var eventType))
        {
            return BadRequest(new { code = "INVALID_PAYLOAD", message = "Webhook body is missing required fields." });
        }

        var result = await _mediator.Send(
            new ReceivePayoutWebhookCommand(providerCode, envelope.EventId, envelope.PayoutReference, eventType),
            cancellationToken);

        // Re-delivery is expected, not an error (specification 5.4) - a duplicate still
        // answers 200 so the provider doesn't keep retrying a webhook we already handled.
        return Ok(new { result.WasDuplicate, PayoutState = result.PayoutState?.ToString() });
    }

    private string ResolveWebhookSecret(ProviderCode providerCode) => providerCode switch
    {
        ProviderCode.Airwallex => _airwallexOptions.Value.WebhookSecret,
        ProviderCode.Rapyd => _rapydOptions.Value.WebhookSecret,
        ProviderCode.Stripe => _stripeOptions.Value.WebhookSecret,
        ProviderCode.Simulated => _simulatedOptions.Value.WebhookSecret,
        _ => throw new ArgumentOutOfRangeException(nameof(providerCode), providerCode, "Unknown provider code."),
    };

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
}
